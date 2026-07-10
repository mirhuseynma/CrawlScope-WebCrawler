import type { PagedResult } from "./crawlJob";

export type Permission = {
  value: string;
  group: string;
  name: string;
};

export type RoleListItem = {
  id: string;
  name: string;
  permissions: string[];
  userCount: number;
  isSystemManaged: boolean;
};

export type RoleDetails = RoleListItem;

export type CreateRoleRequest = {
  name: string;
  permissions: string[];
};

export type UpdateRolePermissionsRequest = {
  permissions: string[];
};

export type UpdateRoleRequest = {
  name: string;
  permissions: string[];
};

export type UserListItem = {
  id: string;
  userName: string;
  email: string;
  fullName: string | null;
  roles: string[];
  isSystemManaged: boolean;
};

export type UserDetails = UserListItem & {
  permissions: string[];
};

export type UpdateUserRolesRequest = {
  roles: string[];
};

export type UpdateUserRequest = {
  userName: string;
  email: string;
  fullName: string | null;
};

export type UsersQuery = {
  search?: string;
  pageNumber: number;
  pageSize: number;
};

export type UsersPageResult = PagedResult<UserListItem>;
