// Site-wide utilities
function formatNumber(n) { return parseFloat(n).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 }); }
function formatInt(n) { return parseInt(n).toLocaleString('en-IN'); }

$(document).ready(function () {
    // Auto-hide TempData alerts after 5s
    setTimeout(function () { $('.alert-dismissible').fadeOut(500); }, 5000);
});
