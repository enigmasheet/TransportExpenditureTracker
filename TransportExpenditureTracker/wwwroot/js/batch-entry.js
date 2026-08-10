const BatchEntry = {
    init: function (opts) {
        const $form = $(opts.form || 'form');
        const $table = $(opts.table);
        const vatRate = parseFloat(opts.vatRate) || 0.13;
        const addCount = opts.addCount || 5;
        const useTemplateRow = opts.useTemplateRow !== false;

        function recalcRow(row) {
            const qty = parseFloat($(row).find('.qty').val()) || 0;
            const rate = parseFloat($(row).find('.rate').val()) || 0;
            const $taxableSpan = $(row).find('.computed-taxable');

            if ($taxableSpan.length) {
                if (qty > 0 && rate > 0) {
                    const taxable = qty * rate;
                    const vat = taxable * vatRate;
                    const total = taxable + vat;
                    $taxableSpan.text(taxable.toFixed(2));
                    $(row).find('.computed-vat').text(vat.toFixed(2));
                    $(row).find('.computed-total').text(total.toFixed(2));
                } else {
                    $taxableSpan.text('—');
                    $(row).find('.computed-vat').text('—');
                    $(row).find('.computed-total').text('—');
                }
                return;
            }

            const $taxableInput = $(row).find('.taxable');
            if (qty > 0 && rate > 0) {
                const taxable = qty * rate;
                $taxableInput.val(taxable.toFixed(2));
                const vat = taxable * vatRate;
                $(row).find('.vat').val(vat.toFixed(2));
                $(row).find('.total').val((taxable + vat).toFixed(2));
            } else {
                const taxable = parseFloat($taxableInput.val()) || 0;
                const vat = taxable * vatRate;
                $(row).find('.vat').val(vat.toFixed(2));
                $(row).find('.total').val((taxable + vat).toFixed(2));
            }
        }

        function initSelect2(container) {
            container.find('.supplier-select').each(function () {
                if (!$(this).data('select2')) {
                    $(this).select2({ width: '100%', allowClear: true });
                }
            });

            container.find('.item-select').each(function () {
                if (!$(this).data('select2')) {
                    $(this).select2({ placeholder: '-- Select Item --', allowClear: true, width: '100%' });
                }
            });
        }

        function initDatepicker(container) {
            container.find('.nepali-datepicker').each(function () {
                if (!$(this).hasClass('ndp-initialized')) {
                    $(this).addClass('ndp-initialized');
                    $(this).nepaliDatePicker({ dateFormat: '%y/%m/%d', ndpYear: true, ndpMonth: true, ndpYearCount: 100 });
                }
            });
        }

        function stripCloneArtifacts(row) {
            row.find('.supplier-select, .item-select').removeClass('select2-hidden-accessible');
            row.find('.nepali-datepicker').removeClass('ndp-initialized');
        }

        function addRows() {
            const tbody = $table.find('tbody');
            const startIdx = tbody.find('tr').length;
            const sourceRow = useTemplateRow
                ? tbody.find('tr.template-row')
                : tbody.find('tr.expense-row, tr.data-row').first();
            if (!sourceRow.length) return;

            for (let i = 0; i < addCount; i++) {
                const idx = startIdx + i;
                const newRow = sourceRow.clone();
                if (useTemplateRow) {
                    newRow.removeClass('template-row').addClass('data-row');
                }
                newRow.find('input, select').val('');
                newRow.find('.computed-taxable, .computed-vat, .computed-total').text('—');
                newRow.find('.row-num').text(idx + 1);
                newRow.find('input, select').each(function () {
                    const name = $(this).attr('name');
                    if (name) $(this).attr('name', name.replace(/\[\d+\]/g, '[' + idx + ']'));
                    const id = $(this).attr('id');
                    if (id) $(this).attr('id', id.replace(/_\d+_/, '_' + idx + '_'));
                });
                stripCloneArtifacts(newRow);
                tbody.append(newRow);
            }

            initSelect2(tbody);
            initDatepicker(tbody);
        }

        function removeLastRow() {
            const rows = $table.find('tbody tr.data-row, tbody tr.expense-row');
            if (rows.length > 1) {
                $(rows[rows.length - 1]).remove();
            } else {
                showToast('warning', 'At least one row is required.');
            }
        }

        function removeRow(btn) {
            const rows = $table.find('tbody tr.data-row, tbody tr.expense-row');
            if (rows.length > 1) $(btn).closest('tr').remove();
            else showToast('warning', 'At least one row is required.');
        }

        $table.on('input', '.qty, .rate, .taxable', function () { recalcRow($(this).closest('tr')); });
        $table.on('click', '.remove-row', function () { removeRow(this); });
        $(opts.addBtn).on('click', addRows);
        if (opts.removeBtn) $(opts.removeBtn).on('click', removeLastRow);

        initSelect2($table.find('tbody'));
        initDatepicker($table.find('tbody'));
        initSelect2($form);
        initDatepicker($form);
        $table.find('tbody tr').each(function () { recalcRow($(this)); });

        var formDirty = false;
        $form.on('change input', 'input, select', function () { formDirty = true; });
        $form.on('submit', function () { formDirty = false; });
        $(window).on('beforeunload', function (e) {
            if (formDirty) {
                e.preventDefault();
                e.returnValue = '';
            }
        });
    }
};