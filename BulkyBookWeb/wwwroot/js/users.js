let userDataTable;

$(document).ready(function () {
    loadDataTable();
})
function loadDataTable() {
    userDataTable = $('#tblData').DataTable({
        ajax: '/admin/user/getall', /* Tal cual esté definido en UserController el método */
        columns: [
            { data: 'name', "width": "25%" },
            { data: 'email', "width": "15%" },
            { data: 'phoneNumber', "width": "10%" },
            {
                data: 'role', "width": "10%", "render": function (data) { return '<span class="badge bg-secondary">' + data + '</span>'; }
            },
            {
                data: 'id', "width": "25%", "render": function (data) {

                    let today = new Date().getTime();
                    let lockedout = new Date(data.lockoutEnd).getTime();
                    let isLocked = lockout > today;

                    return `<div class="d-flex gap-2 justify-content-end">
                            <a onclick="LockUnlock('${data}')" class="btn btn-sm ${isLocked ? 'btn-danger' : 'btn-success'}">
                                 <i class="bi bi-${isLocked ? 'unlock' : 'lock'}-fill"></i> ${isLocked ? 'Unlock' : 'Lock'}
                            </a>
                            <a href="/admin/user/RoleManagement?userId=${data}" class="btn btn-sm btn-outline-secondary">
                                 <i class="bi bi-person-badge"></i> Role
                            </a>
                              <a onclick="ChangePassword('/admin/user/ChangePassword?userId=${data}')" class="btn btn-sm btn-outline-danger">
                                 <i class="bi bi-key-fill"></i> Password
                            </a>
                        </div > `;
                }
            }
        ]
    });
}
