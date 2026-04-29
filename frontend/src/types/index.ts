// Izometri Case Study - shared TypeScript types

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

export type UserRole = 'Admin' | 'HR' | 'Personel';

export type ExpenseStatus = 'Draft' | 'Pending' | 'Approved' | 'Rejected';

export type ExpenseCategory = 'Travel' | 'Equipment' | 'Education' | 'Other';

export type Currency = 'TRY' | 'USD' | 'EUR';

export interface ExpenseDto {
  id: string;
  tenantId: string;
  requestedByUserId: string;
  category: ExpenseCategory;
  currency: Currency;
  amount: number;
  exchangeRate: number;
  amountInTry: number;
  description: string;
  status: ExpenseStatus;
  hrApproved: boolean;
  adminApproved: boolean;
  requiresAdminApproval: boolean;
  rejectionReason?: string;
  submittedAt?: string;
  approvedAt?: string;
  rejectedAt?: string;
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

export interface PagedResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
}

export interface PagedResult<T> extends PagedResponse<T> {
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
  tenantId: string;
  email: string;
  displayName: string;
  roles: string[];
  createdAt: string;
}

export interface CreateUserRequest {
  email: string;
  password: string;
  displayName: string;
  roles: string[];
  phone?: string;
}

export interface UpdateRolesRequest {
  roles: string[];
}

export interface NotificationDto {
  id: string;
  tenantId: string;
  eventId: string;
  eventType: string;
  correlationId: string;
  expenseId: string;
  recipient: string;
  recipientEmail: string;
  recipientPhone: string;
  emailStatus: string;
  emailError?: string;
  message: string;
  sentAt: string;
}

export interface HealthResponse {
  status: string;
}

export interface OutboxDeadLetter {
  id: string;
  eventType: string;
  routingKey: string;
  correlationId: string;
  retryCount: number;
  error?: string;
  createdAt: string;
  deadLetteredAt?: string;
}

export interface NotificationDeadLetterDto {
  id: string;
  eventId: string;
  tenantId?: string;
  expenseId?: string;
  eventType: string;
  routingKey: string;
  correlationId: string;
  error: string;
  retryCount: number;
  deadLetteredAt?: string;
  createdAt: string;
}

export interface ExchangeRateResponse {
  fixedUsdRate?: number | null;
  fixedEurRate?: number | null;
  currentUsdRate: number;
  currentEurRate: number;
}

export interface UpdateRatesRequest {
  fixedUsdRate?: number | null;
  fixedEurRate?: number | null;
}

export interface SendProbeEmailRequest {
  toEmail: string;
  subject: string;
  body: string;
}

export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  errors?: Record<string, string[]>;
}
