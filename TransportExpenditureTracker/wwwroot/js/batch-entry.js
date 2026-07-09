function recalcRow(row) {
    const qty = parseFloat($(row).find('.qty').val()) || 0;
    const rate = parseFloat($(row).find('.rate').val()) || 0;
    const taxableSpan = $(row).find('.computed-taxable');
    const vatSpan = $(row).find('.computed-vat');
    const totalSpan = $(row).find('.computed-total');

    if (qty > 0 && rate > 0) {
        const taxable = qty * rate;
        const vat = taxable * 0.13;
        const total = taxable + vat;
        taxableSpan.text(taxable.toFixed(2));
        vatSpan.text(vat.toFixed(2));
        totalSpan.text(total.toFixed(2));
    } else {
        taxableSpan.text('—');
        vatSpan.text('—');
        totalSpan.text('—');
    }
}

function addBatchRows() {
    const tbody = $('#batch-table tbody');
    const count = tbody.find('tr').length;
    const template = tbody.find('tr.template-row').clone();
    template.removeClass('template-row').addClass('data-row');
    template.find('input, select').val('');
    template.find('.computed-taxable, .computed-vat, .computed-total').text('—');
    template.find('select').each(function () {
        if ($(this).hasClass('supplier-select')) {
            $(this).val('').trigger('change');
        }
    });
    // Strip artifact classes that survived clone so init functions don't skip them
    template.find('.supplier-select, .item-select').removeClass('select2-hidden-accessible');
    template.find('.nepali-datepicker').removeClass('ndp-initialized');

    for (let i = 0; i < 5; i++) {
        const newRow = template.clone();
        const idx = count + i;
        newRow.find('input, select').each(function () {
            const name = $(this).attr('name');
            if (name) $(this).attr('name', name.replace(/\[\d+\]/g, '[' + idx + ']'));
        });
        newRow.find('.row-num').text(idx + 1);
        tbody.append(newRow);
    }

    initRowSelect2(tbody);
    initRowDatepicker(tbody);
}

function removeLastRow() {
    const tbody = $('#batch-table tbody');
    const rows = tbody.find('tr.data-row');
    if (rows.length > 1) {
        $(rows[rows.length - 1]).remove();
    } else {
        showToast('warning', 'At least one row is required.');
    }
}

function initRowSelect2(container) {
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

function initRowDatepicker(container) {
    container.find('.nepali-datepicker').each(function () {
        if (!$(this).hasClass('ndp-initialized')) {
            $(this).addClass('ndp-initialized');
            $(this).nepaliDatePicker({ dateFormat: '%y/%m/%d', ndpYear: true, ndpMonth: true, ndpYearCount: 100 });
        }
    });
}

$(document).ready(function () {
    $('#batch-table').on('input', '.qty, .rate', function () { recalcRow($(this).closest('tr')); });
    $('#add-rows-btn').on('click', addBatchRows);
    $('#remove-row-btn').on('click', removeLastRow);

    initRowSelect2($('#batch-table tbody'));
    initRowDatepicker($('#batch-table tbody'));

    // Recalculate computed columns on page load (validation error return)
    $('#batch-table tbody tr').each(function () { recalcRow($(this)); });
});