import { FormEvent, useEffect, useMemo, useState } from "react";
import { createRole, deleteRole, getPermissions, getRoles, updateRole } from "../api/adminAccessApi";
import { ModalNotification } from "../components/ModalNotification";
import type { Permission, RoleListItem } from "../types/adminAccess";

const rolesPageSize = 8;

function getRoleIcon(roleName: string) {
  const normalizedRole = roleName.toLowerCase();

  if (normalizedRole.includes("admin")) {
    return "A";
  }

  if (normalizedRole.includes("it") || normalizedRole.includes("manager")) {
    return "IT";
  }

  return "R";
}

function groupPermissions(permissions: Permission[]) {
  return permissions.reduce<Record<string, Permission[]>>((groups, permission) => {
    groups[permission.group] = [...(groups[permission.group] ?? []), permission];
    return groups;
  }, {});
}

export function RolesPage() {
  const [roles, setRoles] = useState<RoleListItem[]>([]);
  const [permissions, setPermissions] = useState<Permission[]>([]);
  const [selectedRoleId, setSelectedRoleId] = useState<string | null>(null);
  const [roleName, setRoleName] = useState("");
  const [editRoleName, setEditRoleName] = useState("");
  const [roleSearch, setRoleSearch] = useState("");
  const [rolePageNumber, setRolePageNumber] = useState(1);
  const [permissionSearch, setPermissionSearch] = useState("");
  const [selectedPermissions, setSelectedPermissions] = useState<string[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const selectedRole = roles.find((role) => role.id === selectedRoleId) ?? null;
  const isSelectedRoleSystemManaged = selectedRole?.isSystemManaged ?? false;
  const filteredRoles = useMemo(() => {
    const normalizedSearch = roleSearch.trim().toLowerCase();

    if (!normalizedSearch) {
      return roles;
    }

    return roles.filter((role) => role.name.toLowerCase().includes(normalizedSearch));
  }, [roleSearch, roles]);
  const totalRolePages = Math.max(1, Math.ceil(filteredRoles.length / rolesPageSize));
  const visibleRoles = filteredRoles.slice((rolePageNumber - 1) * rolesPageSize, rolePageNumber * rolesPageSize);
  const filteredPermissions = useMemo(() => {
    const normalizedSearch = permissionSearch.trim().toLowerCase();

    if (!normalizedSearch) {
      return permissions;
    }

    return permissions.filter((permission) =>
      permission.name.toLowerCase().includes(normalizedSearch)
      || permission.group.toLowerCase().includes(normalizedSearch)
      || permission.value.toLowerCase().includes(normalizedSearch),
    );
  }, [permissionSearch, permissions]);


  async function loadData() {
    setIsLoading(true);
    setError(null);

    try {
      const [rolesData, permissionsData] = await Promise.all([getRoles(), getPermissions()]);
      setRoles(rolesData);
      setPermissions(permissionsData);

      const nextSelectedRole = selectedRoleId
        ? rolesData.find((role) => role.id === selectedRoleId)
        : rolesData[0];

      if (nextSelectedRole) {
        setSelectedRoleId(nextSelectedRole.id);
        setSelectedPermissions(nextSelectedRole.permissions);
        setEditRoleName(nextSelectedRole.name);
      }
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to load roles.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void loadData();
  }, []);

  useEffect(() => {
    setRolePageNumber((current) => Math.min(current, totalRolePages));
  }, [totalRolePages]);



  function selectRole(role: RoleListItem) {
    setSelectedRoleId(role.id);
    setSelectedPermissions(role.permissions);
    setEditRoleName(role.name);
    setRoleName("");
    setError(null);
    setSuccess(null);
  }

  function togglePermission(permission: string) {
    setSelectedPermissions((current) =>
      current.includes(permission)
        ? current.filter((item) => item !== permission)
        : [...current, permission],
    );
  }

  function updateRoleSearch(value: string) {
    setRoleSearch(value);
    setRolePageNumber(1);
  }

  function updatePermissionSearch(value: string) {
    setPermissionSearch(value);
  }

  async function handleCreateRole(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsSubmitting(true);
    setError(null);

    try {
      const role = await createRole({
        name: roleName,
        permissions: selectedPermissions,
      });
      await loadData();
      setSelectedRoleId(role.id);
      setSelectedPermissions(role.permissions);
      setEditRoleName(role.name);
      setRoleName("");
      setSuccess(`Role '${role.name}' created successfully.`);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to create role.");
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleUpdateRole() {
    if (!selectedRole) {
      return;
    }

    setIsSubmitting(true);
    setError(null);

    try {
      const role = await updateRole(selectedRole.id, {
        name: editRoleName,
        permissions: selectedPermissions,
      });
      setRoles((current) => current.map((item) => (item.id === role.id ? role : item)));
      setEditRoleName(role.name);
      setSelectedPermissions(role.permissions);
      setSuccess(`Role '${role.name}' updated successfully.`);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to update role.");
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleDeleteRole(role: RoleListItem) {
    const confirmed = window.confirm(`Delete role ${role.name}?`);

    if (!confirmed) {
      return;
    }

    setIsSubmitting(true);
    setError(null);

    try {
      await deleteRole(role.id);
      setSelectedRoleId(null);
      setEditRoleName("");
      setSelectedPermissions([]);
      await loadData();
      setSuccess(`Role '${role.name}' deleted successfully.`);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to delete role.");
    } finally {
      setIsSubmitting(false);
    }
  }

  const allPermissionGroups = useMemo(() => groupPermissions(filteredPermissions), [filteredPermissions]);

  return (
    <section className="stack users-admin-page">
      <div className="section-header users-admin-hero">
        <div>
          <p className="eyebrow">Access control</p>
          <h2>Roles</h2>
          <span>
            {roles.length} roles · {permissions.length} permissions
          </span>
        </div>
        <button className="icon-button" type="button" onClick={() => void loadData()}>
          Refresh
        </button>
      </div>

      <ModalNotification message={error || success} type={error ? "error" : "success"} onClose={() => { setError(null); setSuccess(null); }} />

      <div className="users-layout">
        <div className="users-list-panel">
          <div className="users-list-header">
            <div className="users-list-header-top">
              <span className="users-list-title">Roles Directory</span>
            </div>
            <div className="users-search-row" style={{ gridTemplateColumns: "1fr" }}>
              <input
                aria-label="Search roles"
                placeholder="Search roles..."
                value={roleSearch}
                onChange={(event) => updateRoleSearch(event.target.value)}
              />
            </div>
          </div>

          <div className="users-list-body">
            {isLoading ? (
              <div className="users-detail-empty">Loading roles...</div>
            ) : visibleRoles.length === 0 ? (
              <div className="users-detail-empty">No roles match filters.</div>
            ) : (
              visibleRoles.map((role) => (
                <button
                  className={`users-list-item${selectedRoleId === role.id ? " active" : ""}`}
                  key={role.id}
                  type="button"
                  onClick={() => selectRole(role)}
                >
                  <span className="users-list-avatar" aria-hidden="true" style={{ background: "linear-gradient(135deg, #1f7a5a, #2d9a73)" }}>
                    {getRoleIcon(role.name)}
                  </span>
                  <span className="users-list-meta">
                    <strong>{role.name}</strong>
                    <small>{role.userCount} users assigned</small>
                  </span>
                  {role.isSystemManaged && <span className="users-list-badge muted">System</span>}
                </button>
              ))
            )}
          </div>

          <div className="users-list-footer">
            <div className="users-search-row" style={{ gridTemplateColumns: "auto auto auto", justifyContent: "space-between" }}>
              <span style={{ fontSize: "11px", color: "#8899aa", fontWeight: 800 }}>
                Page {rolePageNumber} of {totalRolePages}
              </span>
              <div style={{ display: "flex", gap: "6px" }}>
                <button
                  className="secondary-button"
                  type="button"
                  disabled={rolePageNumber === 1}
                  onClick={() => setRolePageNumber((current) => Math.max(1, current - 1))}
                  style={{ minHeight: "32px", padding: "0 10px", fontSize: "12px" }}
                >
                  Prev
                </button>
                <button
                  className="secondary-button"
                  type="button"
                  disabled={rolePageNumber === totalRolePages}
                  onClick={() => setRolePageNumber((current) => Math.min(totalRolePages, current + 1))}
                  style={{ minHeight: "32px", padding: "0 10px", fontSize: "12px" }}
                >
                  Next
                </button>
              </div>
            </div>
          </div>
          
          <div className="users-list-header" style={{ borderTop: "1px solid #edf2f7", borderBottom: "none", marginTop: "auto" }}>
            <span className="users-list-title" style={{ marginBottom: "6px", display: "block" }}>Create New Role</span>
            <form className="users-search-row" style={{ gridTemplateColumns: "1fr auto" }} onSubmit={(e) => void handleCreateRole(e)}>
              <input
                aria-label="New role name"
                placeholder="Role name"
                value={roleName}
                onChange={(event) => setRoleName(event.target.value)}
                required
              />
              <button className="primary-button" type="submit" disabled={isSubmitting || !roleName.trim()}>
                Create
              </button>
            </form>
          </div>
        </div>

        <div className="users-detail-panel">
          {!selectedRole ? (
            <div className="users-detail-empty">
              Select a role from the directory to manage its permissions.
            </div>
          ) : (
            <>
              <div className="users-detail-top">
                <div className="users-detail-identity">
                  <span className="users-detail-avatar" aria-hidden="true" style={{ background: "linear-gradient(135deg, #1f7a5a, #2d9a73)" }}>
                    {getRoleIcon(selectedRole.name)}
                  </span>
                  <div className="users-detail-name">
                     <input
                        className="role-name-input"
                        aria-label="Role name"
                        value={editRoleName}
                        disabled={isSelectedRoleSystemManaged}
                        onChange={(event) => setEditRoleName(event.target.value)}
                        style={{ padding: 0, margin: 0, border: "none", background: "transparent", fontSize: "18px", fontWeight: 800, color: "#17202a", outline: "none" }}
                      />
                    <span className="users-detail-email">
                      {selectedRole.userCount} users {selectedRole.isSystemManaged && " · Seed managed"}
                    </span>
                  </div>
                </div>
                <div className="users-detail-actions">
                  {!isSelectedRoleSystemManaged && (
                    <button
                      className="danger-button"
                      type="button"
                      onClick={() => void handleDeleteRole(selectedRole)}
                      disabled={isSubmitting}
                    >
                      Delete role
                    </button>
                  )}
                </div>
              </div>

              <div className="users-detail-body">
                <div>
                  <div className="users-detail-section-label">
                    <p>Permissions Matrix</p>
                    <span>
                      {selectedPermissions.length} selected · {permissions.length} total
                    </span>
                  </div>
                  
                  <div className="users-search-row" style={{ marginBottom: "16px", gridTemplateColumns: "minmax(0, 1fr)" }}>
                    <input
                      aria-label="Search permissions"
                      placeholder="Search permissions..."
                      value={permissionSearch}
                      onChange={(event) => updatePermissionSearch(event.target.value)}
                    />
                  </div>

                  <div className="perm-category-grid">
                    {Object.entries(allPermissionGroups).map(([groupName, groupPerms]) => (
                      <div className="perm-category" key={groupName}>
                        <span className="perm-cat-label">{groupName}</span>
                        <div className="perm-chip-row" style={{ display: "flex", flexDirection: "column", gap: "4px" }}>
                          {groupPerms.map((permission) => (
                            <label
                              className={`perm-chip${selectedPermissions.includes(permission.value) ? " is-checked" : ""}`}
                              key={permission.value}
                            >
                              <input
                                type="checkbox"
                                checked={selectedPermissions.includes(permission.value)}
                                disabled={isSelectedRoleSystemManaged}
                                onChange={() => togglePermission(permission.value)}
                              />
                              <span className="perm-chip-dot" />
                              {permission.name}
                            </label>
                          ))}
                        </div>
                      </div>
                    ))}
                    {Object.keys(allPermissionGroups).length === 0 && (
                      <div className="users-detail-empty" style={{ gridColumn: "1 / -1" }}>
                        No permissions match this search.
                      </div>
                    )}
                  </div>
                </div>
              </div>

              <div className="users-detail-footer">
                <span className="users-detail-email">
                  {isSelectedRoleSystemManaged && "Permissions cannot be modified for system-managed roles."}
                </span>
                <div style={{ display: "flex", gap: "10px" }}>
                  <button
                    className="primary-button"
                    type="button"
                    onClick={() => void handleUpdateRole()}
                    disabled={isSubmitting || isSelectedRoleSystemManaged || !editRoleName.trim()}
                  >
                    Save role
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
