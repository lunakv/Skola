window.onload = function () {
    let deletes = document.getElementsByClassName('delete');
    for (let i = 0; i < deletes.length; i++) {
        let button = deletes[i];
        button.onclick = deleteClick(button);
    }

    let edits = document.getElementsByClassName('edit');
    for (let i = 0; i < edits.length; i++) {
        let button = edits[i];
        button.onclick = editClick(button);
    };

    let confirms = document.getElementsByClassName('confirm');
    for (let i = 0; i < confirms.length; i++) {
        let button = confirms[i];
        button.onclick = sendEdit(button);
    }

    let cancels = document.getElementsByClassName('cancel');
    for (let i = 0; i < cancels.length; i++) {
        let button = cancels[i];
        button.onclick = cancelClick(button);
    }

    let ups = document.getElementsByClassName('up');
    for (let i = 0; i < ups.length; i++) {
        let button = ups[i];
        button.onclick = shiftClick(button);
    }
}

function deleteClick(button) {
    return async function() {
        let response = undefined;
        try {
            response = await fetch("?action=delete", postData(button.value));
        } catch (err) {
            error("Connection to server failed");
            return;
        }

        if (response.ok) {
            let json = await response.json();
            if (json.status == "deleted") {
                button.closest(".item").remove();
                error();
            } else {
                error(json.message);
            }
        } else {
            parseError(response);
        }
    }
}

function editClick(button) {
    return function() {
        let row = button.closest(".item");
        row.getElementsByClassName("amount-input")[0].value =
            row.getElementsByClassName("amount-label")[0].innerHTML;
        showEdit(row, true);
    }
}

function sendEdit(button) {
    return async function() {
        let row = button.closest('.item');
        let input = row.getElementsByClassName("amount-input")[0];
        let response = undefined;
        try {
            response = await fetch("?action=edit", postData(button.value, input.value));
        } catch {
            error("Connection to server failed");
            return;
        }

        if (response.ok) {
            let json = await response.json();
            if (json.status == "updated") {
                row.getElementsByClassName("amount-label")[0]
                    .innerHTML = json.amount;
                showEdit(row, false);
                error();
            } else {
                error(json.message);
            }
        } else {
            parseError(response);
        }
    }
}

function cancelClick(button) {
    let row = button.closest(".item");
    return function() {
        showEdit(row, false);
    }
}

function showEdit(row, show) {
    row.getElementsByClassName("confirm-buttons")[0].hidden = !show;
    row.getElementsByClassName("edit-buttons")[0].hidden = show;
    row.getElementsByClassName("amount-label")[0].hidden = show;
    row.getElementsByClassName("amount-input")[0].type = show ? "number" : "hidden";
}

function shiftClick(button) {
    return async function() {
        let response = undefined;
        try {
            response = await fetch("?action=swap", postData(button.value));
        } catch {
            error("Connection to server failed");
            return;
        }

        if (response.ok) {
            let json = await response.json();
            if (json.status == "shifted") {
                let row = button.closest('.item');
                let before = row.previousElementSibling;
                row.parentNode.insertBefore(row, before);
            } else {
                alert(json.message);
            }
        } else {
            parseError(response);
        }
    }
}

function postData(name, amount) {
    let body = "name="+encodeURIComponent(name);
    if (amount !== null)
        body += "&amount="+encodeURIComponent(amount);

    return {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded'
        },
        body: body
    };
}

function error(message)
{
    let error = document.getElementById("error");
    if (message === undefined)
        error.innerHTML = "";
    else 
        error.innerHTML = message;
}

async function parseError(response) {
    let text = await response.text();
    try {
        let json = JSON.parse(text);
        error(json.message);
    } catch {
        error(text);
    }
}
