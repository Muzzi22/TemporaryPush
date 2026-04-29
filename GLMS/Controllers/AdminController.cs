using GLMS.Web.Data;
using GLMS.Web.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;
        private readonly IAntiforgery _antiforgery;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context,
            IAntiforgery antiforgery)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _antiforgery = antiforgery;
        }

        // The dashboard 
        public async Task<IActionResult> Dashboard()
        {
            ViewBag.TotalUsers = _userManager.Users.Count();
            ViewBag.TotalContracts = await _context.Contracts.CountAsync();
            ViewBag.ActiveContracts = await _context.Contracts.CountAsync(c => c.Status == ContractStatus.Active);
            ViewBag.ExpiredContracts = await _context.Contracts.CountAsync(c => c.Status == ContractStatus.Expired);
            ViewBag.TotalClients = await _context.Clients.CountAsync();
            ViewBag.TotalRequests = await _context.ServiceRequests.CountAsync();
            ViewBag.PendingRequests = await _context.ServiceRequests.CountAsync(sr => sr.Status == RequestStatus.Pending);
            return View();
        }

        // 
        public async Task<IActionResult> Users()
        {
            var users = _userManager.Users.ToList();
            var userRoles = new Dictionary<string, IList<string>>();

            foreach (var u in users)
                userRoles[u.Id] = await _userManager.GetRolesAsync(u);

           
            var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
            string token = tokens.RequestToken ?? "";

            //  Build table rows 
            var rows = "";
            int counter = 1;

            foreach (var user in users)
            {
                var roles = userRoles.ContainsKey(user.Id) ? userRoles[user.Id] : new List<string>();
                bool isAdmin = roles.Contains("Admin");
                bool isCurrent = user.Email == User.Identity!.Name;
                string initial = !string.IsNullOrEmpty(user.FullName)
                                        ? user.FullName.Substring(0, 1).ToUpper()
                                        : (user.Email ?? "?").Substring(0, 1).ToUpper();
                string displayName = string.IsNullOrEmpty(user.FullName) ? "No Name Set" : user.FullName;
                string registered = user.CreatedAt.ToString("dd MMM yyyy");

                string avatarStyle = isAdmin
                    ? "background:#6c63ff;color:#fff;"
                    : "background:#e9ecef;color:#6c757d;";

                string roleBadge = isAdmin
                    ? "<span style='display:inline-block;padding:3px 10px;border-radius:4px;font-size:0.72rem;" +
                      "background:rgba(108,99,255,0.12);color:#6c63ff;border:1px solid rgba(108,99,255,0.35);'>&#128274; Admin</span>"
                    : "<span style='display:inline-block;padding:3px 10px;border-radius:4px;font-size:0.72rem;" +
                      "background:#6c757d;color:#fff;'>&#128100; User</span>";

                string youBadge = isCurrent
                    ? "<span style='display:inline-block;margin-left:4px;padding:1px 6px;border-radius:3px;" +
                      "font-size:0.6rem;background:#6c757d;color:#fff;'>You</span>"
                    : "";

                string actionHtml;
                if (isCurrent)
                {
                    actionHtml = "<span style='color:#adb5bd;font-size:0.85rem;font-style:italic;'>Current account</span>";
                }
                else
                {
                    string promoteOrDemote = isAdmin
                        ? $@"<form method='post' action='/Admin/DemoteToUser' style='display:inline;margin:0;'>
                               <input type='hidden' name='__RequestVerificationToken' value='{token}'/>
                               <input type='hidden' name='userId' value='{user.Id}'/>
                               <button type='submit'
                                       style='font-size:0.75rem;padding:3px 10px;border-radius:4px;
                                              border:1px solid #ffc107;background:transparent;color:#ffc107;cursor:pointer;'
                                       onmouseover=""this.style.background='#ffc107';this.style.color='#000';""
                                       onmouseout=""this.style.background='transparent';this.style.color='#ffc107';""
                                       onclick=""return confirm('Demote {displayName} to regular User?')"">
                                   &#8595; Demote
                               </button>
                             </form>"
                        : $@"<form method='post' action='/Admin/PromoteToAdmin' style='display:inline;margin:0;'>
                               <input type='hidden' name='__RequestVerificationToken' value='{token}'/>
                               <input type='hidden' name='userId' value='{user.Id}'/>
                               <button type='submit'
                                       style='font-size:0.75rem;padding:3px 10px;border-radius:4px;
                                              border:1px solid #212529;background:transparent;color:#212529;cursor:pointer;'
                                       onmouseover=""this.style.background='#212529';this.style.color='#fff';""
                                       onmouseout=""this.style.background='transparent';this.style.color='#212529';""
                                       onclick=""return confirm('Promote {displayName} to Admin?')"">
                                   &#8593; Promote
                               </button>
                             </form>";

                    string assignClientBtn = $@"
                        <a href='/Admin/AssignClient?userId={user.Id}'
                           style='font-size:0.75rem;padding:3px 10px;border-radius:4px;
                                  border:1px solid #0d6efd;background:transparent;color:#0d6efd;
                                  cursor:pointer;text-decoration:none;'>
                            &#128279; Assign Client
                        </a>";

                    string deleteBtn = $@"
                        <form method='post' action='/Admin/DeleteUser' style='display:inline;margin:0;'>
                            <input type='hidden' name='__RequestVerificationToken' value='{token}'/>
                            <input type='hidden' name='userId' value='{user.Id}'/>
                            <button type='submit'
                                    style='font-size:0.75rem;padding:3px 10px;border-radius:4px;
                                           border:1px solid #dc3545;background:transparent;color:#dc3545;cursor:pointer;'
                                    onmouseover=""this.style.background='#dc3545';this.style.color='#fff';""
                                    onmouseout=""this.style.background='transparent';this.style.color='#dc3545';""
                                    onclick=""return confirm('Permanently delete {displayName}? This cannot be undone.')"">
                                &#128465; Delete
                            </button>
                        </form>";

                    actionHtml = $"<div style='display:flex;justify-content:flex-end;align-items:center;gap:8px;'>{promoteOrDemote}{assignClientBtn}{deleteBtn}</div>";
                }

                rows += $@"
                <tr style='border-bottom:1px solid #f0f0f0;'>
                    <td style='padding:12px 0 12px 16px;color:#adb5bd;font-size:0.82rem;'>{counter}</td>
                    <td style='padding:12px 16px;'>
                        <div style='display:flex;align-items:center;gap:10px;'>
                            <div style='width:38px;height:38px;border-radius:50%;{avatarStyle}
                                        display:flex;align-items:center;justify-content:center;
                                        font-size:0.9rem;font-weight:700;flex-shrink:0;'>
                                {initial}
                            </div>
                            <div>
                                <div style='font-weight:600;font-size:0.88rem;'>{displayName}</div>
                                {youBadge}
                            </div>
                        </div>
                    </td>
                    <td style='padding:12px 16px;color:#6c757d;font-size:0.85rem;'>{user.Email}</td>
                    <td style='padding:12px 16px;'>{roleBadge}</td>
                    <td style='padding:12px 16px;color:#6c757d;font-size:0.83rem;'>{registered}</td>
                    <td style='padding:12px 16px;text-align:right;'>{actionHtml}</td>
                </tr>";

                counter++;
            }

            // ── Build success/error alert ──
            string alertHtml = "";
            if (TempData["Success"] is string successMsg)
                alertHtml = $@"<div style='padding:12px 16px;border-radius:8px;background:#d1e7dd;
                                           color:#0a3622;border:1px solid #a3cfbb;margin-bottom:1.5rem;'>
                                    <strong>Done!</strong> {successMsg}
                               </div>";
            else if (TempData["Error"] is string errorMsg)
                alertHtml = $@"<div style='padding:12px 16px;border-radius:8px;background:#f8d7da;
                                           color:#58151c;border:1px solid #f1aeb5;margin-bottom:1.5rem;'>
                                    <strong>Error:</strong> {errorMsg}
                               </div>";

            // ── Full page HTML ──
            string currentUserEmail = User.Identity!.Name ?? "";

            string html = $@"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='utf-8'/>
    <meta name='viewport' content='width=device-width,initial-scale=1.0'/>
    <title>User Management — GLMS</title>
    <link rel='stylesheet' href='/lib/bootstrap/dist/css/bootstrap.min.css'/>
    <style>
        * {{ box-sizing:border-box; margin:0; padding:0; }}
        body {{ background:#f8f9fa; font-family: system-ui, -apple-system, sans-serif; }}
        a {{ text-decoration:none; }}

        /* Navbar */
        .topnav {{
            display:flex; align-items:center; justify-content:space-between;
            background:#212529; padding:0 24px; height:56px;
            border-bottom:1px solid rgba(255,255,255,0.08);
        }}
        .topnav-brand {{
            font-size:1.1rem; font-weight:700; color:#fff; letter-spacing:1px;
        }}
        .topnav-right {{ display:flex; align-items:center; gap:12px; }}
        .pill {{
            background:rgba(108,99,255,0.15); border:1px solid rgba(108,99,255,0.3);
            border-radius:20px; padding:4px 14px; font-size:0.78rem; color:#a89fff;
        }}
        .btn-signout {{
            padding:4px 14px; border-radius:6px; border:1px solid #6c757d;
            background:transparent; color:#adb5bd; font-size:0.82rem; cursor:pointer;
        }}
        .btn-signout:hover {{ background:#6c757d; color:#fff; }}
        .btn-admin-panel {{
            padding:4px 14px; border-radius:6px; border:none;
            background:#ffc107; color:#000; font-size:0.82rem;
            font-weight:600; cursor:pointer; text-decoration:none;
        }}

        /* Layout */
        .layout {{ display:flex; min-height:calc(100vh - 56px); }}

        /* Sidebar */
        .sidebar {{
            width:220px; flex-shrink:0;
            background:#1a1a2e; border-right:1px solid #2a2a4a;
            padding:16px 0;
        }}
        .sidebar-label {{
            font-size:0.62rem; font-weight:700; letter-spacing:2px;
            text-transform:uppercase; color:#3a3a5a;
            padding:8px 20px 4px;
        }}
        .sidebar-link {{
            display:flex; align-items:center; gap:10px;
            padding:9px 20px; font-size:0.855rem; color:#8888aa;
            border-radius:6px; margin:1px 8px;
            transition:background 0.15s, color 0.15s;
        }}
        .sidebar-link:hover {{ background:rgba(255,255,255,0.07); color:#fff; }}
        .sidebar-link.active {{ background:rgba(108,99,255,0.25); color:#fff; }}
        .sidebar-divider {{ border:none; border-top:1px solid #2a2a4a; margin:10px 16px; }}

        /* Main */
        .main {{ flex:1; padding:2rem; }}

        /* Card */
        .card {{
            background:#fff; border-radius:10px;
            box-shadow:0 1px 4px rgba(0,0,0,0.08); overflow:hidden;
        }}

        /* Table */
        table {{ width:100%; border-collapse:collapse; }}
        thead tr {{ background:#212529; }}
        thead th {{
            padding:12px 16px; color:#fff;
            font-size:0.82rem; font-weight:600;
            text-align:left; white-space:nowrap;
        }}
        thead th:last-child {{ text-align:right; }}
        tbody tr:hover {{ background:#f8f9fa; }}
    </style>
</head>
<body>

    <!-- Navbar -->
    <div class='topnav'>
        <a class='topnav-brand' href='/Contracts/Index'>&#9650; GLMS</a>
        <div class='topnav-right'>
            <a href='/Admin/Dashboard' class='btn-admin-panel'>&#128274; Admin Panel</a>
            <span class='pill'>&#9679;&nbsp;{currentUserEmail}</span>
            <form method='post' action='/Account/Logout' style='display:inline;margin:0;'>
                <input type='hidden' name='__RequestVerificationToken' value='{token}'/>
                <button type='submit' class='btn-signout'>Sign Out</button>
            </form>
        </div>
    </div>

    <!-- Layout -->
    <div class='layout'>

        <!-- Sidebar -->
        <div class='sidebar'>
            <div class='sidebar-label'>Admin</div>
            <a class='sidebar-link' href='/Admin/Dashboard'>&#128274; Dashboard</a>
            <a class='sidebar-link active' href='/Admin/Users'>&#128101; Manage Users</a>
            <hr class='sidebar-divider'/>
            <div class='sidebar-label'>Navigation</div>
            <a class='sidebar-link' href='/Clients/Index'>&#128100; Clients</a>
            <a class='sidebar-link' href='/Contracts/Index'>&#128196; Contracts</a>
            <a class='sidebar-link' href='/ServiceRequests/Index'>&#128203; Service Requests</a>
            <hr class='sidebar-divider'/>
            <div class='sidebar-label'>Account</div>
            <span class='sidebar-link' style='cursor:default;font-size:0.78rem;word-break:break-all;'>
                &#9679;&nbsp;{currentUserEmail}
            </span>
        </div>

        <!-- Main content -->
        <div class='main'>

            <!-- Page header -->
            <div style='display:flex;justify-content:space-between;align-items:flex-start;margin-bottom:1.5rem;'>
                <div>
                    <a href='/Admin/Dashboard'
                       style='font-size:0.82rem;color:#6c757d;'>&#8592; Admin Dashboard</a>
                    <h2 style='margin:4px 0 2px;font-size:1.5rem;font-weight:700;'>User Management</h2>
                    <p style='font-size:0.85rem;color:#6c757d;margin:0;'>
                        Manage roles and access for all registered users
                    </p>
                </div>
                <span style='background:#212529;color:#fff;border-radius:6px;
                             padding:6px 14px;font-size:0.9rem;font-weight:600;'>
                    {users.Count} Users
                </span>
            </div>

            {alertHtml}

            <!-- Table card -->
            <div class='card'>
                <table>
                    <thead>
                        <tr>
                            <th style='width:50px;'>#</th>
                            <th>Name</th>
                            <th>Email</th>
                            <th>Role</th>
                            <th>Registered</th>
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {(rows == "" ? "<tr><td colspan='6' style='text-align:center;padding:2rem;color:#adb5bd;'>No users found.</td></tr>" : rows)}
                    </tbody>
                </table>
            </div>

        </div>
    </div>

    <script src='/lib/bootstrap/dist/js/bootstrap.bundle.min.js'></script>
</body>
</html>";

            return Content(html, "text/html");
        }

        // promote 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoteToAdmin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            if (!await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _userManager.RemoveFromRoleAsync(user, "User");
                await _userManager.AddToRoleAsync(user, "Admin");
                TempData["Success"] = $"{user.FullName} has been promoted to Admin.";
            }
            else
            {
                TempData["Error"] = $"{user.FullName} is already an Admin.";
            }

            return Redirect("/Admin/Users");
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DemoteToUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Id == userId)
            {
                TempData["Error"] = "You cannot demote yourself.";
                return Redirect("/Admin/Users");
            }

            await _userManager.RemoveFromRoleAsync(user, "Admin");
            await _userManager.AddToRoleAsync(user, "User");
            TempData["Success"] = $"{user.FullName} has been demoted to User.";

            return Redirect("/Admin/Users");
        }

        //Delete 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Id == userId)
            {
                TempData["Error"] = "You cannot delete your own account.";
                return Redirect("/Admin/Users");
            }

            var result = await _userManager.DeleteAsync(user);
            TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
                ? $"{user.FullName} has been permanently deleted."
                : "Failed to delete user. Please try again.";

            return Redirect("/Admin/Users");
        }

        // expiring of old contracts
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExpireOldContracts()
        {
            var expired = await _context.Contracts
                .Where(c => c.EndDate < DateTime.Today && c.Status == ContractStatus.Active)
                .ToListAsync();

            foreach (var c in expired)
                c.Status = ContractStatus.Expired;

            await _context.SaveChangesAsync();
            TempData["Success"] = $"{expired.Count} contract(s) marked as expired.";

            return Redirect("/Admin/Dashboard");
        }

        // show assigned client form
        public async Task<IActionResult> AssignClient(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();
            ViewBag.Clients = await _context.Clients.ToListAsync();
            ViewBag.TargetUser = user;
            return View("~/Views/Admin/AssignClient.cshtml", user);
        }

        // save client
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignClient(string userId, int? clientId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();
            user.ClientId = clientId;
            await _userManager.UpdateAsync(user);
            var clientName = "None";
            if (clientId.HasValue)
            {
                var client = await _context.Clients.FindAsync(clientId.Value);
                clientName = client?.Name ?? "None";
            }
            TempData["Success"] = $"{user.FullName} linked to client: {clientName}.";
            return Redirect("/Admin/Users");
        }

        // viewing of contracts through status controls
        public async Task<IActionResult> Contracts()
        {
            var contracts = await _context.Contracts
                .Include(c => c.Client)
                .OrderByDescending(c => c.Id)
                .ToListAsync();
            return View("~/Views/Admin/Contracts.cshtml", contracts);
        }

        // updating the status of your contract
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateContractStatus(
            int contractId, GLMS.Web.Models.ContractStatus newStatus)
        {
            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.Id == contractId);
            if (contract == null) return NotFound();
            var oldStatus = contract.Status;
            contract.Status = newStatus;
            await _context.SaveChangesAsync();
            TempData["Success"] =
                $"Contract #{contractId} status updated to {newStatus}.";
            return RedirectToAction(nameof(Contracts));
        }
    }
}