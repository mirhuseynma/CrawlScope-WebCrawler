import { request } from "./httpClient";
import type {
  CreateRoleRequest,
  Permission,
  RoleDetails,
  RoleListItem,
  UpdateRolePermissionsRequest,
  UpdateRoleRequest,
  UpdateUserRequest,
  UpdateUserRolesRequest,
  UserDetails,
  UsersPageResult,
  UsersQuery,
} from "../types/adminAccess";

function toQueryString(query: Record<string, string | number | undefined>) {
  const params = new URLSearchParams();

  Object.entries(query).forEach(([key, value]) => {
    if (value !== undefined && value !== "") {
      params.set(key, String(value));
    }
  });

  const value = params.toString();
  return value ? `?${value}` : "";
}

export function getPermissions() {
  return request<Permission[]>("/api/AdminRoles/permissions");
}

export function getRoles() {
  return request<RoleListItem[]>("/api/AdminRoles");
}

export function createRole(payload: CreateRoleRequest) {
  return request<RoleDetails>("/api/AdminRoles", {
    method: "POST",
    body: payload,
  });
}

export function updateRolePermissions(roleId: string, payload: UpdateRolePermissionsRequest) {
  return request<RoleDetails>(`/api/AdminRoles/${roleId}/permissions`, {
    method: "PUT",
    body: payload,
  });
}

export function updateRole(roleId: string, payload: UpdateRoleRequest) {
  return request<RoleDetails>(`/api/AdminRoles/${roleId}`, {
    method: "PUT",
    body: payload,
  });
}

export function deleteRole(roleId: string) {
  return request<void>(`/api/AdminRoles/${roleId}`, {
    method: "DELETE",
  });
}

export function getUsers(query: UsersQuery) {
  return request<UsersPageResult>(
    `/api/AdminUsers${toQueryString({
      search: query.search,
      pageNumber: query.pageNumber,
      pageSize: query.pageSize,
    })}`,
  );
}

export function getUserById(userId: string) {
  return request<UserDetails>(`/api/AdminUsers/${userId}`);
}

export function updateUser(userId: string, payload: UpdateUserRequest) {
  return request<UserDetails>(`/api/AdminUsers/${userId}`, {
    method: "PUT",
    body: payload,
  });
}

export function updateUserRoles(userId: string, payload: UpdateUserRolesRequest) {
  return request<UserDetails>(`/api/AdminUsers/${userId}/roles`, {
    method: "PUT",
    body: payload,
  });
}

export function deleteUser(userId: string) {
  return request<void>(`/api/AdminUsers/${userId}`, {
    method: "DELETE",
  });
}
