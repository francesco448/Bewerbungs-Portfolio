// Seiten-Editor fuer /Admin?edit=true
//
// Der Inhalt wird nicht in einer JavaScript-Kopie gehalten, sondern jedes Mal
// direkt aus dem DOM gelesen. Die Partials markieren dafuer:
//   data-section="hero"     ein Bereich
//   data-field="kicker"     ein Textfeld (Element-Text oder Wert eines <input>)
//   data-multiline="true"   Zeilenumbrueche im Text erhalten
//   data-object="button"    ein verschachteltes Objekt
//   data-list="items"       eine Liste, deren Eintraege data-item tragen
//   data-string-list="true" Liste aus reinen Texten statt Objekten
//   data-image="image"      ein Bild, Pfad steht in data-value
//   data-flag="wide"        eine Checkbox fuer einen Ja/Nein-Wert
//   data-file="documents"   Knopf, der eine Datei hochlaedt und den Pfad ins Feld schreibt
//   data-max="500"          Zeichengrenze eines contenteditable-Feldes, aus
//                           ContentLimits gerendert (siehe Models/SiteContent)
//
// Dadurch braucht es keine Index-Verwaltung: nach Verschieben oder Loeschen
// ergibt sich die Reihenfolge wieder aus der Reihenfolge im DOM.

(function () {
    "use strict";

    var root = document.getElementById("page-editor");
    if (!root || !root.classList.contains("pe-editing")) {
        return;
    }

    var statusBox = document.getElementById("pe-status");
    var fileInput = document.getElementById("pe-file-input");
    var loginBox = document.getElementById("pe-login");
    var loginError = document.getElementById("pe-login-error");
    var loginSubmit = document.getElementById("pe-login-submit");
    var tokenField = document.querySelector('input[name="__RequestVerificationToken"]');
    var token = tokenField ? tokenField.value : "";

    var saveTimer = null;
    var pendingRetry = null;

    // ---------- Status ----------

    function setStatus(text, state) {
        if (!statusBox) return;
        statusBox.textContent = text;
        statusBox.className = "pe-status" + (state ? " is-" + state : "");
    }

    // ---------- Aus dem DOM lesen ----------

    // Das Element, zu dem ein Feld gehoert: der naechste Eintrag, das naechste
    // Objekt oder der Bereich darueber.
    function ownerOf(node) {
        return node.parentElement.closest("[data-item], [data-object], [data-section]");
    }

    function ownedBy(container, selector) {
        return Array.prototype.filter.call(
            container.querySelectorAll(selector),
            function (node) { return ownerOf(node) === container; });
    }

    function itemsOf(listElement) {
        return Array.prototype.filter.call(
            listElement.querySelectorAll("[data-item]"),
            function (node) { return node.parentElement.closest("[data-list]") === listElement; });
    }

    // Gleiche Normalisierung wie beim Speichern: bei einzeiligen Feldern
    // werden mehrere Leer- bzw. Zeilenumbruchzeichen zu einem Leerzeichen.
    // Der Zeichenzaehler misst dieselbe Laenge, damit er nicht etwas anderes
    // anzeigt als das, was tatsaechlich gespeichert wird.
    function normalizeText(text, multiline) {
        if (!multiline) {
            text = text.replace(/\s+/g, " ");
        }
        return text.trim();
    }

    function readValue(node) {
        if (node.tagName === "INPUT" || node.tagName === "TEXTAREA") {
            return node.value;
        }
        return normalizeText(node.innerText, node.dataset.multiline === "true");
    }

    function collect(container) {
        var result = {};

        ownedBy(container, "[data-field]").forEach(function (node) {
            result[node.dataset.field] = readValue(node);
        });

        ownedBy(container, "[data-image]").forEach(function (node) {
            result[node.dataset.image] = node.dataset.value || "";
        });

        ownedBy(container, "[data-flag]").forEach(function (node) {
            result[node.dataset.flag] = node.checked === true;
        });

        ownedBy(container, "[data-object]").forEach(function (node) {
            result[node.dataset.object] = collect(node);
        });

        // Listen: nur die, die direkt zu diesem Container gehoeren
        Array.prototype.filter.call(
            container.querySelectorAll("[data-list]"),
            function (node) { return ownerOf(node) === container; }
        ).forEach(function (listElement) {
            result[listElement.dataset.list] = collectList(listElement);
        });

        return result;
    }

    function collectList(listElement) {
        var asStrings = listElement.dataset.stringList === "true";
        return itemsOf(listElement).map(function (item) {
            return asStrings ? readValue(item) : collect(item);
        });
    }

    function collectAll() {
        var content = {};
        root.querySelectorAll("[data-section]").forEach(function (section) {
            content[section.dataset.section] = collect(section);
        });
        return content;
    }

    // ---------- Zeichenzaehler ----------
    //
    // Jedes contenteditable-Feld mit data-max bekommt einen Zaehler
    // "aktuell / maximal" direkt danach und wird an genau dieser Grenze
    // gekappt -- die Zahl kommt serverseitig aus ContentLimits und ist
    // dieselbe, die auch [StringLength] beim Speichern durchsetzt.
    //
    // Ein MutationObserver haelt das auch fuer Felder aktuell, die erst nach
    // dem Laden entstehen (neuer Listeneintrag, neu geladener Bereich nach
    // saveAndRefresh/putAndRefresh) -- ohne dass jede Stelle, die das DOM
    // aendert, den Zaehler von Hand anstossen muesste.

    function maxOf(field) {
        var max = parseInt(field.dataset.max, 10);
        return isNaN(max) ? null : max;
    }

    function ensureCounter(field) {
        var next = field.nextElementSibling;
        if (next && next.classList.contains("pe-charcount")) {
            return next;
        }
        var counter = document.createElement("span");
        counter.className = "pe-charcount";
        field.insertAdjacentElement("afterend", counter);
        return counter;
    }

    function updateCounter(field) {
        var max = maxOf(field);
        if (max === null) return;

        var length = normalizeText(field.innerText, field.dataset.multiline === "true").length;
        var counter = ensureCounter(field);
        counter.textContent = length + " / " + max;
        counter.classList.toggle("is-warning", length >= max * 0.8 && length < max);
        counter.classList.toggle("is-limit", length >= max);
    }

    // Kappt den Rohtext an der Grenze. Bewusst am rohen innerText statt am
    // normalisierten Wert, damit die gespeicherte (normalisierte) Laenge nie
    // ueber dem serverseitigen Limit liegen kann.
    function enforceMax(field) {
        var max = maxOf(field);
        if (max === null) return;

        var raw = field.innerText;
        if (raw.length <= max) return;

        field.innerText = raw.slice(0, max);
        placeCaretAtEnd(field);
    }

    function placeCaretAtEnd(field) {
        var range = document.createRange();
        range.selectNodeContents(field);
        range.collapse(false);
        var selection = window.getSelection();
        selection.removeAllRanges();
        selection.addRange(range);
    }

    var charCountObserver = new MutationObserver(function () {
        refreshAllCounters();
    });

    function refreshAllCounters() {
        // Waehrend des eigenen Umbaus nicht auf die dabei entstehenden
        // Aenderungen (neue Zaehler-Spans) reagieren -- sonst beobachtet
        // sich der Observer selbst und laeuft endlos.
        charCountObserver.disconnect();

        root.querySelectorAll('[contenteditable="true"][data-max]').forEach(updateCounter);

        charCountObserver.observe(root, { childList: true, subtree: true });
    }

    // ---------- Fehler einordnen ----------

    // Wird eine abgelaufene Sitzung erkannt, wirft diese Funktion einen Fehler
    // mit sessionExpired = true; der Aufrufer blendet dann die Anmeldung ein.
    // Bei allen anderen Fehlern kommt eine lesbare Meldung heraus statt eines
    // JSON-Parserfehlers -- 401 und 413 liefern naemlich einen leeren Body.
    function describeError(response) {
        if (response.status === 401) {
            var expired = new Error("Sitzung abgelaufen");
            expired.sessionExpired = true;
            return Promise.resolve(expired);
        }

        if (response.status === 413) {
            return Promise.resolve(new Error("Die Datei ist zu gross."));
        }

        return response.text().then(function (body) {
            // Eine zu grosse Datei meldet ASP.NET als Validierungsfehler mit
            // englischem Text -- daraus wird hier eine verstaendliche Meldung.
            if (/request body too large|exceeded the request limit/i.test(body)) {
                return new Error("Die Datei ist zu gross.");
            }

            if (body) {
                try {
                    var parsed = JSON.parse(body);

                    // Eigene Meldungen des Controllers
                    if (parsed && parsed.error) {
                        return new Error(parsed.error);
                    }

                    // ProblemDetails: erste Fehlermeldung herausziehen. Der
                    // Schluessel (z. B. "Skills.Items[2].Text") sagt, welches
                    // Feld betroffen ist -- reportError springt damit direkt
                    // dorthin, statt nur eine allgemeine Meldung zu zeigen.
                    if (parsed && parsed.errors) {
                        for (var key in parsed.errors) {
                            var messages = parsed.errors[key];
                            if (messages && messages.length) {
                                var validationError = new Error(messages[0]);
                                validationError.fieldPath = key;
                                return validationError;
                            }
                        }
                    }
                } catch (ignored) {
                    // Kein JSON -- dann bleibt es bei der allgemeinen Meldung.
                }
            }

            return new Error("Fehler " + response.status);
        });
    }

    function reportError(error) {
        if (error.sessionExpired) {
            requireLogin(error.retry);
            return;
        }
        setStatus("Nicht gespeichert – " + error.message, "error");

        if (error.fieldPath) {
            highlightField(error.fieldPath);
        }
    }

    // ---------- Zum fehlerhaften Feld springen ----------
    //
    // Wandelt einen Validierungs-Schluessel wie "Skills.Items[2].Text" oder
    // "Projects.Items[4].Details[0].Heading" in den passenden DOM-Knoten um.
    // Die Segmente entsprechen genau den data-section/-list/-item/-object/
    // -field-Attributen, nur in PascalCase statt camelCase -- derselbe Weg,
    // den collect() in die andere Richtung geht.

    function decodeSegment(raw) {
        var match = raw.match(/^([^[\]]+)(?:\[(\d+)\])?$/);
        if (!match) return null;
        var name = match[1].charAt(0).toLowerCase() + match[1].slice(1);
        var index = match[2] !== undefined ? parseInt(match[2], 10) : null;
        return { name: name, index: index };
    }

    function locateField(fieldPath) {
        var segments = fieldPath.split(".").map(decodeSegment);
        if (segments.some(function (s) { return s === null; })) return null;

        var first = segments.shift();
        var node = root.querySelector('[data-section="' + first.name + '"]');

        for (var i = 0; node && i < segments.length; i++) {
            var seg = segments[i];

            if (seg.index !== null) {
                // Liste: erst den Listencontainer im aktuellen Knoten finden,
                // dann den Eintrag an der gemeldeten Stelle.
                var list = ownedBy(node, '[data-list="' + seg.name + '"]')[0];
                node = list ? (itemsOf(list)[seg.index] || list) : null;
            } else {
                // Feld oder verschachteltes Objekt.
                node = ownedBy(node, '[data-field="' + seg.name + '"]')[0] ||
                    ownedBy(node, '[data-object="' + seg.name + '"]')[0] ||
                    null;
            }
        }

        return node;
    }

    function highlightField(fieldPath) {
        var node = locateField(fieldPath);
        if (!node) return;

        node.scrollIntoView({ behavior: "smooth", block: "center" });
        node.classList.add("pe-field-error");
        window.setTimeout(function () { node.classList.remove("pe-field-error"); }, 2500);

        if (node.getAttribute("contenteditable") === "true") {
            node.focus();
        }
    }

    // ---------- Speichern ----------

    // Schickt einen fertigen Inhalt. Schlaegt es an der Sitzung fehl, merkt sich
    // der Fehler genau diesen Inhalt, damit nach dem Anmelden derselbe Stand
    // erneut gesendet wird -- auch eine gerade hinzugefuegte Karte.
    function putContent(content) {
        setStatus("Speichern …", "saving");

        return fetch("/api/v1/pagecontent", {
            method: "PUT",
            headers: {
                "Content-Type": "application/json",
                "RequestVerificationToken": token
            },
            body: JSON.stringify(content)
        }).then(function (response) {
            if (!response.ok) {
                return describeError(response).then(function (error) { throw error; });
            }
            setStatus("Gespeichert " + new Date().toLocaleTimeString("de-CH"), "saved");
        });
    }

    function save() {
        return putContent(collectAll()).catch(function (error) {
            error.retry = save;
            reportError(error);
            throw error;
        });
    }

    function saveSoon() {
        setStatus("Nicht gespeichert …");
        window.clearTimeout(saveTimer);
        saveTimer = window.setTimeout(save, 600);
    }

    // ---------- Bereich neu rendern ----------

    // Nach einer Strukturaenderung laesst der Server den Bereich neu rendern.
    // Das haelt Reihenfolge, Kachel-Varianten und Zeilenaufteilung korrekt,
    // ohne die ganze Seite neu zu laden.
    function refreshSection(name) {
        return fetch("/Admin?handler=Section&name=" + encodeURIComponent(name), {
            headers: { "X-Requested-With": "fetch" }
        }).then(function (response) {
            if (!response.ok) {
                throw new Error("HTTP " + response.status);
            }
            return response.text();
        }).then(function (html) {
            var current = root.querySelector('[data-section="' + name + '"]');
            if (!current) return;

            var holder = document.createElement("div");
            holder.innerHTML = html;
            var replacement = holder.querySelector('[data-section="' + name + '"]');
            if (replacement) {
                current.replaceWith(replacement);
            }
        });
    }

    // Speichern und danach den betroffenen Bereich neu aufbauen.
    function saveAndRefresh(sectionName) {
        window.clearTimeout(saveTimer);
        save().then(function () {
            return refreshSection(sectionName);
        }).catch(function () {
            // Fehlermeldung steht bereits im Status.
        });
    }

    function sectionNameOf(node) {
        var section = node.closest("[data-section]");
        return section ? section.dataset.section : null;
    }

    // ---------- Eingaben ----------

    root.addEventListener("input", function (event) {
        var target = event.target;
        var field = target.closest("[contenteditable='true']");

        if (field) {
            if (field.dataset.max) {
                enforceMax(field);
                updateCounter(field);
            }
            saveSoon();
            return;
        }

        if (target.matches("input[data-field], textarea[data-field]")) {
            // Icon-Vorschau sofort mitziehen
            if (target.matches("input[data-field='icon']")) {
                var item = target.closest("[data-item]");
                var icon = item && item.querySelector(".techno-service-icon i, .contact-us-icon i");
                if (icon) {
                    icon.className = target.value;
                }
            }

            saveSoon();
        }
    });

    root.addEventListener("change", function (event) {
        if (event.target.matches("input[data-flag]")) {
            saveAndRefresh(sectionNameOf(event.target));
        }
    });

    // Beim Verlassen eines Feldes sofort sichern statt auf den Timer zu warten.
    root.addEventListener("focusout", function (event) {
        if (event.target.closest("[contenteditable='true']") && saveTimer) {
            window.clearTimeout(saveTimer);
            save().catch(function () { });
        }
    });

    // Zeilenumbrueche nur dort zulassen, wo das Feld mehrzeilig ist.
    root.addEventListener("keydown", function (event) {
        if (event.key !== "Enter") return;

        var field = event.target.closest("[contenteditable='true']");
        if (field && field.dataset.multiline !== "true") {
            event.preventDefault();
        }
    });

    // Formatierten Text beim Einfuegen auf reinen Text reduzieren.
    root.addEventListener("paste", function (event) {
        if (!event.target.closest("[contenteditable='true']")) return;

        event.preventDefault();
        var text = (event.clipboardData || window.clipboardData).getData("text/plain");
        document.execCommand("insertText", false, text);
    });

    // ---------- Listen: hinzufuegen, verschieben, loeschen ----------

    root.addEventListener("click", function (event) {
        var addButton = event.target.closest(".pe-add");
        if (addButton) {
            event.preventDefault();
            addItem(addButton);
            return;
        }

        var controlButton = event.target.closest(".pe-item-controls button");
        if (controlButton) {
            event.preventDefault();
            handleItemControl(controlButton);
            return;
        }

        var fileButton = event.target.closest(".pe-file");
        if (fileButton) {
            event.preventDefault();
            pickFile(fileButton);
            return;
        }

        var image = event.target.closest("[data-image]");
        if (image) {
            event.preventDefault();
            pickImage(image);
        }
    });

    function listForButton(button) {
        // Der Knopf steht innerhalb des Listencontainers oder direkt daneben.
        var name = button.dataset.add;
        var section = button.closest("[data-section]");
        var candidates = section.querySelectorAll('[data-list="' + name + '"]');

        for (var i = 0; i < candidates.length; i++) {
            if (candidates[i].contains(button) ||
                candidates[i].parentElement === button.parentElement ||
                button.closest("[data-item]") === candidates[i].closest("[data-item]")) {
                return candidates[i];
            }
        }
        return candidates[0] || null;
    }

    function addItem(button) {
        var listElement = listForButton(button);
        if (!listElement) return;

        var content = collectAll();
        var target = resolveList(content, listElement);
        if (!target) return;

        // Ein leeres Objekt genuegt: der Server fuellt beim Einlesen die
        // Standardwerte aus dem C#-Modell ein.
        target.array.push(listElement.dataset.stringList === "true" ? "" : {});

        putAndRefresh(content, sectionNameOf(button));
    }

    function handleItemControl(button) {
        var item = button.closest("[data-item]");
        var listElement = item.parentElement.closest("[data-list]");
        if (!listElement) return;

        var index = itemsOf(listElement).indexOf(item);
        if (index < 0) return;

        var content = collectAll();
        var target = resolveList(content, listElement);
        if (!target) return;

        var array = target.array;

        if (button.classList.contains("pe-delete")) {
            array.splice(index, 1);
        } else if (button.classList.contains("pe-move-up") && index > 0) {
            array.splice(index - 1, 0, array.splice(index, 1)[0]);
        } else if (button.classList.contains("pe-move-down") && index < array.length - 1) {
            array.splice(index + 1, 0, array.splice(index, 1)[0]);
        } else {
            return;
        }

        putAndRefresh(content, sectionNameOf(button));
    }

    // Findet im eingesammelten Objekt die Liste, die zu einem DOM-Container gehoert.
    // Der Weg wird ueber dieselbe Zugehoerigkeit gebildet wie beim Einsammeln,
    // damit ineinander liegende Listen korrekt aufgeloest werden.
    function resolveList(content, listElement) {
        var path = [listElement.dataset.list];
        var owner = ownerOf(listElement);

        while (owner && owner.dataset.section === undefined) {
            if (owner.dataset.item !== undefined) {
                var parentList = owner.parentElement.closest("[data-list]");
                if (!parentList) return null;
                path.unshift(itemsOf(parentList).indexOf(owner));
                path.unshift(parentList.dataset.list);
                owner = ownerOf(parentList);
            } else if (owner.dataset.object !== undefined) {
                path.unshift(owner.dataset.object);
                owner = ownerOf(owner);
            } else {
                return null;
            }
        }

        if (!owner) return null;
        path.unshift(owner.dataset.section);

        var current = content;
        for (var i = 0; i < path.length; i++) {
            current = current[path[i]];
            if (current === undefined || current === null) return null;
        }

        return Array.isArray(current) ? { array: current } : null;
    }

    function putAndRefresh(content, sectionName) {
        window.clearTimeout(saveTimer);

        putContent(content).then(function () {
            return refreshSection(sectionName);
        }).catch(function (error) {
            error.retry = function () { putAndRefresh(content, sectionName); };
            reportError(error);
        });
    }

    // ---------- Bilder und Dokumente ----------

    // Merkt sich, was nach dem Hochladen aktualisiert werden soll.
    var pendingTarget = null;

    function pickImage(image) {
        if (!fileInput) return;
        pendingTarget = { kind: "image", element: image, endpoint: "images" };
        fileInput.accept = "image/*";
        fileInput.value = "";
        fileInput.click();
    }

    function pickFile(button) {
        if (!fileInput) return;

        var row = button.closest(".pe-file-row");
        var field = row && row.querySelector("input[data-field]");
        if (!field) return;

        pendingTarget = {
            kind: "file",
            element: field,
            endpoint: button.dataset.file || "documents",
            item: button.closest("[data-item]")
        };
        fileInput.accept = button.dataset.accept || "";
        fileInput.value = "";
        fileInput.click();
    }

    function upload(file, target) {
        var data = new FormData();
        data.append("file", file);

        setStatus((target.kind === "image" ? "Bild" : "Datei") + " wird hochgeladen …", "saving");

        return fetch("/api/v1/pagecontent/" + target.endpoint, {
            method: "POST",
            headers: { "RequestVerificationToken": token },
            body: data
        }).then(function (response) {
            if (!response.ok) {
                return describeError(response).then(function (error) { throw error; });
            }
            return response.json();
        }).then(function (body) {
            if (target.kind === "image") {
                target.element.src = body.url;
                target.element.dataset.value = body.url;
            } else {
                target.element.value = body.url;

                // Der sichtbare Link soll sofort auf die neue Datei zeigen.
                var link = target.item && target.item.querySelector(".circular-btn a");
                if (link) {
                    link.href = body.url;
                }
            }
            return save();
        }).catch(function (error) {
            error.retry = function () { upload(file, target); };
            reportError(error);
        });
    }

    if (fileInput) {
        fileInput.addEventListener("change", function () {
            var file = fileInput.files && fileInput.files[0];
            var target = pendingTarget;
            pendingTarget = null;
            if (!file || !target) return;

            upload(file, target);
        });
    }

    // ---------- Erneut anmelden, ohne die Seite zu verlassen ----------

    // Bei einer abgelaufenen Sitzung wird hier angemeldet und danach genau die
    // Aktion wiederholt, die fehlgeschlagen ist. Die Seite bleibt stehen, alle
    // Eingaben bleiben erhalten.
    function requireLogin(retry) {
        if (!loginBox) {
            setStatus("Sitzung abgelaufen – bitte Seite neu laden", "error");
            return;
        }

        pendingRetry = retry || null;
        setStatus("Sitzung abgelaufen", "error");
        loginError.hidden = true;
        loginBox.hidden = false;

        var user = document.getElementById("pe-login-user");
        if (user) {
            user.focus();
        }
    }

    // Holt ein frisches Antiforgery-Token von der Login-Seite: das Token der
    // Editor-Seite gehoert zur inzwischen abgelaufenen Anmeldung.
    function submitLogin() {
        var user = document.getElementById("pe-login-user");
        var password = document.getElementById("pe-login-pass");
        if (!user.value || !password.value) {
            showLoginError("Bitte Benutzername und Passwort eingeben.");
            return;
        }

        loginSubmit.disabled = true;
        loginError.hidden = true;

        fetch("/Login", { headers: { "X-Requested-With": "fetch" } })
            .then(function (response) { return response.text(); })
            .then(function (html) {
                var match = html.match(/name="__RequestVerificationToken"[^>]*?value="([^"]+)"/);
                if (!match) {
                    throw new Error("Login-Seite nicht lesbar");
                }

                var body = new URLSearchParams();
                body.set("Username", user.value);
                body.set("Password", password.value);
                body.set("__RequestVerificationToken", match[1]);

                return fetch("/Login", {
                    method: "POST",
                    headers: { "Content-Type": "application/x-www-form-urlencoded" },
                    body: body.toString(),
                    redirect: "manual"
                });
            })
            .then(function (response) {
                // Erfolg beantwortet der Server mit einer Weiterleitung; bei
                // falschen Daten kommt die Login-Seite mit Status 200 zurueck.
                if (response.type !== "opaqueredirect" && response.status !== 302) {
                    throw new Error("Benutzername oder Passwort ist falsch.");
                }

                loginBox.hidden = true;
                password.value = "";
                loginSubmit.disabled = false;

                var retry = pendingRetry;
                pendingRetry = null;
                if (retry) {
                    retry();
                } else {
                    setStatus("Wieder angemeldet", "saved");
                }
            })
            .catch(function (error) {
                loginSubmit.disabled = false;
                showLoginError(error.message);
            });
    }

    function showLoginError(message) {
        if (!loginError) return;
        loginError.textContent = message;
        loginError.hidden = false;
    }

    if (loginSubmit) {
        loginSubmit.addEventListener("click", submitLogin);
    }

    if (loginBox) {
        loginBox.addEventListener("keydown", function (event) {
            if (event.key === "Enter") {
                event.preventDefault();
                submitLogin();
            }
        });
    }

    // ---------- Ungespeicherte Aenderungen ----------

    window.addEventListener("beforeunload", function (event) {
        if (saveTimer) {
            event.preventDefault();
            event.returnValue = "";
        }
    });

    // Zaehler fuer alle beim Laden vorhandenen Felder anzeigen und den
    // Observer fuer spaeter hinzukommende Felder scharf schalten.
    refreshAllCounters();

    setStatus("Bereit");
})();
