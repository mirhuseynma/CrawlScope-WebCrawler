import { FormEvent, useEffect, useMemo, useState } from "react";
import { deleteUser, getRoles, getUsers, updateUser, updateUserRoles } from "../api/adminAccessApi";
import { PaginationControls } from "../components/PaginationControls";
import type { RoleListItem, UserListItem, UsersPageResult } from "../types/adminAccess";

const emptyUsersPage: UsersPageResult = {
  items: [],
  pageNumber: 1,
  pageSize: 10,
  totalCount: 0,
  totalPages: 0,
  hasPreviousPage: false,
  hasNextPage: false,
};

const rolePageSize = 6;

function getRoleIcon(roleName: string) {
  const normalizedRole = roleName.toLowerCase();

  if (normalizedRole.includes("admin")) {
    return "A";
  }

  if (normalizedRole.includes("it") || normalizedRole.includes("manager")) {
    return "IT";
  }

  return "U";
}

function getInitials(user: UserListItem) {
  const source = user.fullName || user.userName || user.email;
  return source
    .split(/[.\s_-]+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase())
    .join("");
}

export function UsersPage() {
  const [usersPage, setUsersPage] = useState<UsersPageResult>(emptyUsersPage);
  const [roles, setRoles] = useState<RoleListItem[]>([]);
  const [selectedUserId, setSelectedUserId] = useState<string | null>(null);
  const [selectedRoles, setSelectedRoles] = useState<string[]>([]);
  const [userForm, setUserForm] = useState({ userName: "", email: "", fullName: "" });
  const [search, setSearch] = useState("");
  const [roleSearch, setRoleSearch] = useState("");
  const [rolePageNumber, setRolePageNumber] = useState(1);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const selectedUser = useMemo(
    () => usersPage.items.find((user) => user.id === selectedUserId) ?? null,
    [selectedUserId, usersPage.items],
  );
  const isSelectedUserSystemManaged = selectedUser?.isSystemManaged ?? false;
  const filteredRoles = useMemo(() => {
    const normalizedSearch = roleSearch.trim().toLowerCase();

    if (!normalizedSearch) {
      return roles;
    }

    return roles.filter((role) => role.name.toLowerCase().includes(normalizedSearch));
  }, [roleSearch, roles]);
  const totalRolePages = Math.max(1, Math.ceil(filteredRoles.length / rolePageSize));
  const visibleRoles = filteredRoles.slice((rolePageNumber - 1) * rolePageSize, rolePageNumber * rolePageSize);

  async function loadData(nextPageNumber = pageNumber) {
    setIsLoading(true);
    setError(null);

    try {
      const [usersData, rolesData] = await Promise.all([
        getUsers({
          search,
          pageNumber: nextPageNumber,
          pageSize,
        }),
        getRoles(),
      ]);

      setUsersPage(usersData);
      setRoles(rolesData);
      setPageNumber(usersData.pageNumber);

      if (!selectedUserId && usersData.items.length > 0) {
        setSelectedUserId(usersData.items[0].id);
        setSelectedRoles(usersData.items[0].roles);
        setUserForm({
          userName: usersData.items[0].userName,
          email: usersData.items[0].email,
          fullName: usersData.items[0].fullName ?? "",
        });
      }
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to load users.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void loadData(1);
  }, []);

  useEffect(() => {
    setRolePageNumber((current) => Math.min(current, totalRolePages));
  }, [totalRolePages]);

  function applyFilters(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    void loadData(1);
  }

  function selectUser(user: UserListItem) {
    setSelectedUserId(user.id);
    setSelectedRoles(user.roles);
    setUserForm({
      userName: user.userName,
      email: user.email,
      fullName: user.fullName ?? "",
    });
    setError(null);
  }

  function toggleRole(roleName: string) {
    setSelectedRoles((current) =>
      current.includes(roleName)
        ? current.filter((role) => role !== roleName)
        : [...current, roleName],
    );
  }

  function updateRoleSearch(value: string) {
    setRoleSearch(value);
    setRolePageNumber(1);
  }

  async function handleSaveRoles() {
    if (!selectedUser) {
      return;
    }

    setIsSubmitting(true);
    setError(null);

    try {
      const user = await updateUserRoles(selectedUser.id, {
        roles: selectedRoles,
      });
      setUsersPage((current) => ({
        ...current,
        items: current.items.map((item) => (item.id === user.id ? user : item)),
      }));
      setSelectedRoles(user.roles);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to update user roles.");
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleSaveProfile() {
    if (!selectedUser) {
      return;
    }

    setIsSubmitting(true);
    setError(null);

    try {
      const user = await updateUser(selectedUser.id, {
        userName: userForm.userName,
        email: userForm.email,
        fullName: userForm.fullName.trim() ? userForm.fullName : null,
      });
      setUsersPage((current) => ({
        ...current,
        items: current.items.map((item) => (item.id === user.id ? user : item)),
      }));
      setUserForm({
        userName: user.userName,
        email: user.email,
        fullName: user.fullName ?? "",
      });
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to update user.");
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleDeleteUser() {
    if (!selectedUser) {
      return;
    }

    const confirmed = window.confirm(`Delete user ${selectedUser.email}?`);

    if (!confirmed) {
      return;
    }

    setIsSubmitting(true);
    setError(null);

    try {
      await deleteUser(selectedUser.id);
      setSelectedUserId(null);
      setSelectedRoles([]);
      setUserForm({ userName: "", email: "", fullName: "" });
      await loadData(pageNumber);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to delete user.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <section className="stack users-admin-page">
      <div className="section-header users-admin-hero">
        <div>
          <p className="eyebrow">Access control</p>
          <h2>Users</h2>
          <span>{usersPage.totalCount} accounts · {roles.length} roles</span>
        </div>
        <button className="icon-button" type="button" onClick={() => void loadData(pageNumber)}>
          Refresh
        </button>
      </div>

      {error && <div className="alert">{error}</div>}

      <div className="users-layout">
        <div className="users-list-panel">
          <div className="users-list-header">
            <div className="users-list-header-top">
              <span className="users-list-title">Users Directory</span>
            </div>
            <form className="users-search-row" onSubmit={applyFilters}>
              <input
                aria-label="Search users"
                placeholder="Search name or email..."
                value={search}
                onChange={(event) => setSearch(event.target.value)}
              />
              <select
                aria-label="Users page size"
                value={pageSize}
                onChange={(event) => setPageSize(Number(event.target.value))}
              >
                <option value={5}>5 / page</option>
                <option value={10}>10 / page</option>
                <option value={25}>25 / page</option>
                <option value={50}>50 / page</option>
              </select>
              <button className="secondary-button" type="submit">
                Apply
              </button>
            </form>
          </div>

          <div className="users-list-body">
            {isLoading ? (
              <div className="users-detail-empty">Loading users...</div>
            ) : usersPage.items.length === 0 ? (
              <div className="users-detail-empty">No users match filters.</div>
            ) : (
              usersPage.items.map((user) => (
                <button
                  className={`users-list-item${selectedUserId === user.id ? " active" : ""}`}
                  key={user.id}
                  type="button"
                  onClick={() => selectUser(user)}
                >
                  <span className="users-list-avatar" aria-hidden="true">
                    {getInitials(user)}
                  </span>
                  <span className="users-list-meta">
                    <strong>{user.fullName || user.userName}</strong>
                    <small>{user.email}</small>
                  </span>
                  {user.isSystemManaged ? (
                    <span className="users-list-badge muted">System</span>
                  ) : (
                    <span className="users-list-badge">{user.roles[0] ?? "User"}</span>
                  )}
                </button>
              ))
            )}
          </div>

          <div className="users-list-footer">
            <PaginationControls
              label="Users"
              page={usersPage}
              onPageChange={(nextPageNumber) => void loadData(nextPageNumber)}
            />
          </div>
        </div>

        <div className="users-detail-panel">
          {!selectedUser ? (
            <div className="users-detail-empty">
              Select a user from the directory to manage their profile and access roles.
            </div>
          ) : (
            <>
              <div className="users-detail-top">
                <div className="users-detail-identity">
                  <span className="users-detail-avatar" aria-hidden="true">
                    {getInitials(selectedUser)}
                  </span>
                  <div className="users-detail-name">
                    <span className="users-detail-username">
                      {selectedUser.fullName || selectedUser.userName}
                    </span>
                    <span className="users-detail-email">
                      {selectedUser.email} {selectedUser.isSystemManaged && " · Seed managed"}
                    </span>
                  </div>
                </div>
                <div className="users-detail-actions">
                  <button
                    className="danger-button"
                    type="button"
                    onClick={() => void handleDeleteUser()}
                    disabled={isSubmitting || isSelectedUserSystemManaged}
                  >
                    Delete user
                  </button>
                </div>
              </div>

              <div className="users-detail-body">
                <div>
                  <div className="users-detail-section-label">
                    <p>Profile Information</p>
                  </div>
                  <div className="users-detail-fields">
                    <label>
                      Full name
                      <input
                        aria-label="Full name"
                        value={userForm.fullName}
                        disabled={isSelectedUserSystemManaged}
                        onChange={(event) =>
                          setUserForm((current) => ({ ...current, fullName: event.target.value }))
                        }
                      />
                    </label>
                    <label>
                      Username
                      <input
                        aria-label="Username"
                        value={userForm.userName}
                        disabled={isSelectedUserSystemManaged}
                        onChange={(event) =>
                          setUserForm((current) => ({ ...current, userName: event.target.value }))
                        }
                      />
                    </label>
                    <label className="users-detail-field-full">
                      Email
                      <input
                        aria-label="Email"
                        type="email"
                        value={userForm.email}
                        disabled={isSelectedUserSystemManaged}
                        onChange={(event) =>
                          setUserForm((current) => ({ ...current, email: event.target.value }))
                        }
                      />
                    </label>
                  </div>
                </div>

                <div>
                  <div className="users-detail-section-label">
                    <p>Access Roles</p>
                    <span>
                      {selectedRoles.length} assigned · {filteredRoles.length} available
                    </span>
                  </div>
                  <div className="users-search-row" style={{ marginBottom: "12px", gridTemplateColumns: "minmax(0, 1fr)" }}>
                    <input
                      aria-label="Search roles"
                      placeholder="Search roles..."
                      value={roleSearch}
                      onChange={(event) => updateRoleSearch(event.target.value)}
                    />
                  </div>

                  {visibleRoles.length === 0 ? (
                    <div className="users-detail-empty" style={{ padding: "20px" }}>
                      No roles match this search.
                    </div>
                  ) : (
                    <div className="users-role-chips">
                      {visibleRoles.map((role) => (
                        <label
                          key={role.id}
                          className={`users-role-chip${selectedRoles.includes(role.name) ? " is-checked" : ""}`}
                        >
                          <input
                            type="checkbox"
                            checked={selectedRoles.includes(role.name)}
                            disabled={isSelectedUserSystemManaged}
                            onChange={() => toggleRole(role.name)}
                          />
                          <span className="users-role-chip-dot" />
                          {role.name}
                        </label>
                      ))}
                    </div>
                  )}

                  {totalRolePages > 1 && (
                    <div className="users-search-row" style={{ marginTop: "12px", gridTemplateColumns: "auto auto auto", justifyContent: "flex-end" }}>
                       <span style={{ fontSize: "11px", color: "#8899aa", fontWeight: 800, paddingRight: "8px" }}>
                         Page {rolePageNumber} of {totalRolePages}
                       </span>
                       <button
                        className="secondary-button"
                        type="button"
                        disabled={rolePageNumber === 1}
                        onClick={() => setRolePageNumber((current) => Math.max(1, current - 1))}
                       >
                         Prev
                       </button>
                       <button
                        className="secondary-button"
                        type="button"
                        disabled={rolePageNumber === totalRolePages}
                        onClick={() => setRolePageNumber((current) => Math.min(totalRolePages, current + 1))}
                       >
                         Next
                       </button>
                    </div>
                  )}
                </div>
              </div>

              <div className="users-detail-footer">
                <span className="users-detail-email">
                  {isSelectedUserSystemManaged && "Profile and roles cannot be modified for system-managed accounts."}
                </span>
                <div style={{ display: "flex", gap: "10px" }}>
                  <button
                    className="secondary-button"
                    type="button"
                    onClick={() => void handleSaveProfile()}
                    disabled={isSubmitting || isSelectedUserSystemManaged || !userForm.userName.trim() || !userForm.email.trim()}
                  >
                    Save profile
                  </button>
                  <button
                    className="primary-button"
                    type="button"
                    onClick={() => void handleSaveRoles()}
                    disabled={isSubmitting || isSelectedUserSystemManaged}
                  >
                    Save roles
                  </button>
                </div>
              </div>
            </>
          )}
        </div>
      </div>
    </section>
  );
}

