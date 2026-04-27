// İzometri Case Study — Shared TypeScript Types

export interface LoginRequest {
  email: string;
  password: string;
  tenantCode: string;
}

export interface LoginResponse {
  accessToken: string;
  userId: string;
  tenantId: string;
  email: string;
  displayName: string;
  roles: string[];
}

export interface JwtPayload {
  sub: string;
  email: string;
  tenantId: string;
  tenantCode: string;
  role: string | string[];
  exp: number;
}

export type UserRole = 'Admin' | 'HR' | 'Personnel';

export type ExpenseStatus =
  | 'Draft'
  | 'Pending'
  | 'PendingAdminApproval'
  | 'Approved'
  | 'Rejected';

export type ExpenseCategory = 'Travel' | 'Equipment' | 'Education' | 'Other';

export type Currency = 'TRY' | 'USD' | 'EUR';

export interface ExpenseDto {
  id: string;
  category: ExpenseCategory;
  currency: Currency;
  amount: number;
  description: string;
  status: ExpenseStatus;
  hrApproved: boolean;
  adminApproved: boolean;
  createdAt: string;
  updatedAt?: string;
  requestedByUserId: string;
  createdByEmail?: string;
  approvals?: ExpenseApprovalDto[];
}

export interface ExpenseApprovalDto {
  id: string;
  approverRole: string;
  isApproved: boolean;
  comment?: string;
  createdAt: string;
}

export interface CreateExpenseRequest {
  category: ExpenseCategory;
  currency: Currency;
  amount: number;
  description: string;
}

export interface UpdateExpenseRequest {
  category: ExpenseCategory;
  currency: Currency;
  amount: number;
  description: string;
}

export interface RejectExpenseRequest {
  reason: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface ExpenseQueryParams {
  dateFrom?: string | undefined;
  dateTo?: string | undefined;
  status?: ExpenseStatus | undefined;
  category?: ExpenseCategory | undefined;
  pageNumber?: number | undefined;
  pageSize?: number | undefined;
}

export interface UserDto {
  id: string;
  email: string;
  fullName: string;
  roles: string[];
  createdAt: string;
}

export interface CreateUserRequest {
  email: string;
  password: string;
  fullName: string;
  roles: string[];
}

export interface UpdateRolesRequest {
  roles: string[];
}

export interface NotificationDto {
  id: string;
  tenantId: string;
  eventType: string;
  expenseId: string;
  message: string;
  recipientEmail?: string;
  emailStatus?: string;
  emailError?: string;
  sentAt: string;
}

export interface HealthResponse {
  status: string;
}

export interface OutboxDeadLetter {
  id: string;
  eventType: string;
  routingKey: string;
  error: string;
  retryCount: number;
  deadLetteredAt: string;
}
