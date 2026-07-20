// ModalForm — reusable modal CRUD handler
var ModalForm = {
    // For static-content modals (Create, Quick-create)
    initCreate: function (options) {
        var modalId = options.modalId;
        var formId = options.formId;
        var submitUrl = options.submitUrl;
        var useFormData = options.useFormData;
        var onSuccess = options.onSuccess || function () { showToast('success', 'Saved successfully.'); setTimeout(function () { location.reload(); }, 500); };

        $('#' + formId).on('submit', function (e) {
            e.preventDefault();
            var $form = $(this);
            if ($form.valid && !$form.valid()) return;

            var $btn = $form.find('button[type="submit"]');
            $btn.loading(true);

            var ajaxOptions = {
                url: submitUrl,
                type: 'POST',
                headers: { 'RequestVerificationToken': $form.find('[name="__RequestVerificationToken"]').val() }
            };

            if (useFormData) {
                ajaxOptions.data = $form.serialize();
            } else {
                ajaxOptions.contentType = 'application/json';
                ajaxOptions.data = JSON.stringify(ModalForm._formData($form));
            }

            $.ajax(ajaxOptions).done(function (data) {
                if (data.success) {
                    $('#' + modalId).modal('hide');
                    $form[0].reset();
                    if (onSuccess) onSuccess(data);
                } else {
                    var msgs = [];
                    $.each(data.errors, function (f, arr) { msgs.push(arr.join(', ')); });
                    showToast('error', msgs.join('<br>'));
                }
            }).fail(function () { showToast('error', 'Operation failed.'); })
              .always(function () { $btn.loading(false); });
        });

        $('#' + modalId).on('hidden.bs.modal', function () {
            $('#' + formId)[0].reset();
        });
    },

    // For dynamic-content modals (Edit, Delete, Roles)
    initDynamic: function (options) {
        var modalId = options.modalId;
        var formId = options.formId;
        var loadUrl = options.loadUrl;
        var submitUrl = options.submitUrl;
        var useFormData = options.useFormData;
        var onSuccess = options.onSuccess || function () { showToast('success', 'Saved successfully.'); setTimeout(function () { location.reload(); }, 500); };

        // Load content when modal opens
        $('#' + modalId).on('show.bs.modal', function (e) {
            var btn = e.relatedTarget;
            var id = $(btn).data('id');
            var paramName = options.loadParamName || 'id';
            var $body = $(this).find('.modal-body');
            $body.html('<div class="text-center py-3"><div class="spinner-border" role="status"></div></div>');

            $.get(loadUrl + '?' + paramName + '=' + encodeURIComponent(id), function (html) {
                $body.html(html);
                if ($.validator && $.validator.unobtrusive) {
                    $.validator.unobtrusive.parse($body);
                }
            });
        });

        // Handle form submission via delegation (content loaded dynamically)
        $(document).on('submit', '#' + formId, function (e) {
            e.preventDefault();
            var $form = $(this);
            if ($form.valid && !$form.valid()) return;

            var $btn = $form.find('button[type="submit"]');
            $btn.loading(true);

            var ajaxOptions = {
                url: submitUrl,
                type: 'POST',
                headers: { 'RequestVerificationToken': $form.find('[name="__RequestVerificationToken"]').val() }
            };

            if (useFormData) {
                ajaxOptions.data = $form.serialize();
            } else {
                ajaxOptions.contentType = 'application/json';
                ajaxOptions.data = JSON.stringify(ModalForm._formData($form));
            }

            $.ajax(ajaxOptions).done(function (data) {
                if (data.success) {
                    $('#' + modalId).modal('hide');
                    if (onSuccess) onSuccess(data);
                } else {
                    var msgs = [];
                    $.each(data.errors, function (f, arr) { msgs.push(arr.join(', ')); });
                    showToast('error', msgs.join('<br>'));
                }
            }).fail(function () { showToast('error', 'Operation failed.'); })
              .always(function () { $btn.loading(false); });
        });
    },

    // Serialize form to plain object
    _formData: function ($form) {
        var data = {};
        $.each($form.serializeArray(), function (_, item) {
            data[item.name] = item.value;
        });
        return data;
    }
};