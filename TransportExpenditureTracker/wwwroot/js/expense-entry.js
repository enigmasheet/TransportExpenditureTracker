function recalcRow(row) {
    const qty = parseFloat($(row).find('.qty').val()) || 0;
    const rate = parseFloat($(row).find('.rate').val()) || 0;
    const taxableInput = $(row).find('.taxable');
    const vatInput = $(row).find('.vat');
    const totalInput = $(row).find('.total');

    if (qty > 0 && rate > 0) {
        const taxable = qty * rate;
        taxableInput.val(taxable.toFixed(2));
        const vat = taxable * 0.13;
        vatInput.val(vat.toFixed(2));
        totalInput.val((taxable + vat).toFixed(2));
    } else {
        const taxable = parseFloat(taxableInput.val()) || 0;
        const vat = taxable * 0.13;
        vatInput.val(vat.toFixed(2));
        totalInput.val((taxable + vat).toFixed(2));
    }
}

function addRow() {
    const tbody = $('#details-table tbody');
    const idx = tbody.find('tr').length;
    const firstRow = tbody.find('tr').first();
    const newRow = firstRow.clone();

    newRow.find('input, select').each(function () {
        $(this).val('');
        if ($(this).attr('name')) $(this).attr('name', $(this).attr('name').replace(/\[\d+\]/g, '[' + idx + ']'));
        if ($(this).attr('id')) $(this).attr('id', $(this).attr('id').replace(/\d+/g, idx));
    });
    newRow.find('.remove-row').show();
    tbody.append(newRow);
    initSelect2(newRow);
}

function removeRow(btn) {
    const tbody = $('#details-table tbody');
    if (tbody.find('tr').length > 1) {
        $(btn).closest('tr').remove();
    } else {
        alert('At least one detail row is required.');
    }
}

function initSelect2(container) {
    $(container).find('select').not('.select2-hidden-accessible').each(function () {
        if ($(this).hasClass('item-select')) {
            $(this).select2({ placeholder: '-- Select Item --', allowClear: true, width: '100%' });
        }
    });
}

$(document).ready(function () {
    $('#details-table').on('input', '.qty, .rate, .taxable', function () { recalcRow($(this).closest('tr')); });
    $('#add-row-btn').on('click', addRow);
    $('#details-table').on('click', '.remove-row', function () { removeRow(this); });
});
