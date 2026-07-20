function formatNumber(n) { return parseFloat(n).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 }); }
function formatInt(n) { return parseInt(n).toLocaleString('en-IN'); }

function showToast(type, message) {
    const iconMap = { success: 'bi-check-circle-fill text-success', error: 'bi-exclamation-circle-fill text-danger', warning: 'bi-exclamation-triangle-fill text-warning', info: 'bi-info-circle-fill text-primary' };
    const icon = iconMap[type] || iconMap.info;
    const delay = type === 'error' ? 8000 : type === 'warning' ? 6000 : 3000;
    const toastHtml = '<div class="toast align-items-center border-0" role="alert" aria-live="assertive" aria-atomic="true" data-bs-delay="' + delay + '">'
        + '<div class="d-flex"><div class="toast-body"><i class="bi ' + icon + ' fs-5"></i> '
        + message
        + '</div><button type="button" class="btn-close me-2 m-auto" data-bs-dismiss="toast"></button></div></div>';
    const $toast = $(toastHtml).appendTo('#toastContainer');
    new bootstrap.Toast($toast[0]).show();
    $toast.on('hidden.bs.toast', function () { $(this).remove(); });
}

$.fn.transportLoading = function (state) {
    if (state) { this.addClass('is-loading').prop('disabled', true); }
    else { this.removeClass('is-loading').prop('disabled', false); }
    return this;
};

$(document).ready(function () {
    const currentPath = (window.location.pathname || '').toLowerCase().replace(/\/+$/, '');
    $('.navbar-nav a').each(function () {
        const href = $(this).attr('href');
        if (href && href !== '#' && currentPath === href.toLowerCase().split('?')[0].replace(/\/+$/, '')) {
            $(this).addClass('active');
            if ($(this).closest('.dropdown-menu').length) {
                $(this).closest('.dropdown').find('.dropdown-toggle').addClass('active');
            }
        }
    });

    const $td = $('#tempData');
    if ($td.length) {
        const success = $td.data('success');
        const error = $td.data('error');
        const warning = $td.data('warning');
        if (success) showToast('success', success);
        if (error) showToast('error', error);
        if (warning) showToast('warning', warning);
    }

    $(document).on('submit', 'form', function (e) {
        if (e.isDefaultPrevented()) return;
        const $btn = $(this).find('button[type="submit"]');
        if ($btn.length && !$btn.hasClass('is-loading')) {
            $btn.transportLoading(true);
        }
    });
});
