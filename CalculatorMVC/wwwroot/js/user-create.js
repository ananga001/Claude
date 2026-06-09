(function () {
    const appData = document.getElementById('app-data').dataset;
    const managers    = JSON.parse(appData.managers);
    const supervisors = JSON.parse(appData.supervisors);

    const ROLE_NORMAL     = appData.roleNormal;
    const ROLE_SUPERVISOR = appData.roleSupervisor;
    const ROLE_MANAGER    = appData.roleManager;

    const roleSelect      = document.getElementById('roleSelect');
    const reportingGroup  = document.getElementById('reportingToGroup');
    const reportingSelect = document.getElementById('reportingToSelect');
    const warning         = document.getElementById('noOptionsWarning');

    function updateReportingDropdown() {
        const role = roleSelect.value;
        reportingSelect.innerHTML = '<option value="">— Select —</option>';
        warning.classList.add('d-none');

        if (role === ROLE_MANAGER) {
            reportingGroup.style.display = 'none';
            reportingSelect.value = '';
            return;
        }

        reportingGroup.style.display = '';
        const options = role === ROLE_SUPERVISOR ? managers : supervisors;

        if (options.length === 0) {
            warning.textContent = role === ROLE_SUPERVISOR
                ? 'No managers exist yet. Please add a Manager first.'
                : 'No supervisors exist yet. Please add a Supervisor first.';
            warning.classList.remove('d-none');
            return;
        }

        options.forEach(function (o) {
            const opt = document.createElement('option');
            opt.value = o.id;
            opt.textContent = o.name;
            reportingSelect.appendChild(opt);
        });
    }

    roleSelect.addEventListener('change', updateReportingDropdown);
    updateReportingDropdown();
}());
