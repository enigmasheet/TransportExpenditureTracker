const ModalForm = {
    initCreate: function (options) {
        const modalId = options.modalId;
        const formId = options.formId;
        const submitUrl = options.submitUrl;
        const useFormData = options.useFormData;
        const onSuccess = options.onSuccess || function () { showToast('success', 'Saved successfully.'); setTimeout(function () { location.reload(); }, 500); };

        $('#' + formId).on('submit', function (e) {
            e.preventDefault();
            const $form = $(this);
            if ($form.valid && !$form.valid()) return;

            const $btn = $form.find('button[type="submit"]');
            $btn.transportLoading(true);

            const ajaxOptions = {
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
                    const msgs = [];
                    $.each(data.errors, function (f, arr) { msgs.push(arr.join(', ')); });
                    showToast('error', msgs.join('<br>'));
                }
            }).fail(function () { showToast('error', 'Operation failed.'); })
              .always(function () { $btn.transportLoading(false); });
        });

        $('#' + modalId).on('hidden.bs.modal', function () {
            $('#' + formId)[0].reset();
        });
    },

    initDynamic: function (options) {
        const modalId = options.modalId;
        const formId = options.formId;
        const loadUrl = options.loadUrl;
        const submitUrl = options.submitUrl;
        const useFormData = options.useFormData;
        const onSuccess = options.onSuccess || function () { showToast('success', 'Saved successfully.'); setTimeout(function () { location.reload(); }, 500); };

        $('#' + modalId).on('show.bs.modal', function (e) {
            const btn = e.relatedTarget;
            const id = $(btn).data('id');
            const paramName = options.loadParamName || 'id';
            const $body = $(this).find('.modal-body');
            $body.html('<div class="text-center py-3"><div class="spinner-border" role="status"></div></div>');

            $.get(loadUrl + '?' + paramName + '=' + encodeURIComponent(id), function (html) {
                $body.html(html);
                if ($.validator && $.validator.unobtrusive) {
                    $.validator.unobtrusive.parse($body);
                }
            });
        });

        $('#' + modalId).on('hidden.bs.modal', function () {
            $('#' + formId)[0].reset();
        });

        $(document).on('submit', '#' + formId, function (e) {
            e.preventDefault();
            const $form = $(this);
            if ($form.valid && !$form.valid()) return;

            const $btn = $form.find('button[type="submit"]');
            $btn.transportLoading(true);

            const ajaxOptions = {
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
                    const msgs = [];
                    $.each(data.errors, function (f, arr) { msgs.push(arr.join(', ')); });
                    showToast('error', msgs.join('<br>'));
                }
            }).fail(function () { showToast('error', 'Operation failed.'); })
              .always(function () { $btn.transportLoading(false); });
        });
    },

    _formData: function ($form) {
        const data = {};
        $.each($form.serializeArray(), function (_, item) {
            data[item.name] = item.value;
        });
        return data;
    }
};
