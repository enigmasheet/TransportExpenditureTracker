function openAddPartyModal() {
    const modalBody = document.getElementById("addPartyModalBody");
    modalBody.innerHTML = `
        <div class="text-center py-5">
            <div class="spinner-border" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
        </div>`;

    fetch('/Parties/GetAddPartyPartial')
        .then(res => res.ok ? res.text() : Promise.reject("Failed to load partial"))
        .then(html => {
            modalBody.innerHTML = html;
            if (window.jQuery && $.validator) {
                $.validator.unobtrusive.parse(modalBody);
            }
            bindPartyFormSubmit();
        })
        .catch(err => {
            modalBody.innerHTML = `<div class="text-danger text-center py-3">Error loading form. ${err}</div>`;
        });

    new bootstrap.Modal(document.getElementById('addPartyModal')).show();
}

function bindPartyFormSubmit() {
    const form = document.getElementById("partyForm");
    const messageDiv = document.getElementById("partyFormMessages");
    if (!form) return;

    form.addEventListener("submit", async function (e) {
        e.preventDefault();

        messageDiv.innerHTML = "";
        const formData = new FormData(form);
        const url = form.getAttribute("action") || "/Parties/Create";

        try {
            const res = await fetch(url, {
                method: "POST",
                body: formData,
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                }
            });

            const data = await res.json();

            if (data.success) {
                messageDiv.innerHTML = `<div class="alert alert-success">${data.message}</div>`;
                form.reset();

                setTimeout(() => {
                    const modal = bootstrap.Modal.getInstance(document.getElementById('addPartyModal'));
                    modal.hide();
                    location.reload();
                }, 1000);
            } else {
                messageDiv.innerHTML = `
                    <div class="alert alert-danger">
                        <ul>${(data.errors || ["Unknown error"]).map(err => `<li>${err}</li>`).join("")}</ul>
                    </div>`;
            }
        } catch (err) {
            messageDiv.innerHTML = `<div class="alert alert-danger">Submission failed. Please try again.</div>`;
        }
    });
}
