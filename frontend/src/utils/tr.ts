// Izometri - Türkçe etiketler ve yardımcı fonksiyonlar

import type { Currency, ExpenseCategory, ExpenseStatus } from 'src/types';

export const statusLabels: Record<ExpenseStatus, string> = {
  Draft: 'Taslak',
  Pending: 'Onay Bekliyor',
  Approved: 'Onaylandı',
  Rejected: 'Reddedildi',
};

export const statusClasses: Record<ExpenseStatus, string> = {
  Draft: 'draft',
  Pending: 'pending',
  Approved: 'approved',
  Rejected: 'rejected',
};

export const categoryLabels: Record<ExpenseCategory, string> = {
  Travel: 'Seyahat',
  Equipment: 'Ekipman',
  Education: 'Eğitim',
  Other: 'Diğer',
};

export const currencySymbols: Record<Currency, string> = {
  TRY: '₺',
  USD: '$',
  EUR: '€',
};

export const roleLabels: Record<string, string> = {
  Admin: 'Yönetici',
  HR: 'İnsan Kaynakları',
  Personel: 'Personel',
};

export function formatAmount(amount: number, currency: Currency): string {
  return `${currencySymbols[currency] ?? ''}${amount.toLocaleString('tr-TR')}`;
}

export function formatDate(iso?: string | null): string {
  if (!iso) return '-';
  return new Date(iso).toLocaleDateString('tr-TR', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  });
}

export function formatDateTime(iso?: string | null): string {
  if (!iso) return '-';
  return new Date(iso).toLocaleString('tr-TR', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

export function translateCategory(cat: ExpenseCategory): string {
  return categoryLabels[cat] ?? cat;
}

export function translateStatus(status: ExpenseStatus): string {
  return statusLabels[status] ?? status;
}

export function translateRole(role: string): string {
  return roleLabels[role] ?? role;
}
