import { defineStore } from 'pinia';
import { api } from 'src/boot/axios';
import type {
  ExpenseDto,
  CreateExpenseRequest,
  RejectExpenseRequest,
  ExpenseQueryParams,
} from 'src/types';

export const useExpenseStore = defineStore('expense', {
  state: () => ({
    expenses: [] as ExpenseDto[],
    currentExpense: null as ExpenseDto | null,
    totalCount: 0,
    pageNumber: 1,
    pageSize: 10,
    totalPages: 0,
    loading: false,
  }),

  actions: {
    async fetchExpenses(params?: ExpenseQueryParams) {
      this.loading = true;
      try {
        const query = new URLSearchParams();
        if (params?.dateFrom) query.set('dateFrom', params.dateFrom);
        if (params?.dateTo) query.set('dateTo', params.dateTo);
        if (params?.status) query.set('status', params.status);
        if (params?.category) query.set('category', params.category);
        query.set('pageNumber', String(params?.pageNumber ?? this.pageNumber));
        query.set('pageSize', String(params?.pageSize ?? this.pageSize));

        const { data } = await api.get(`/expenses?${query.toString()}`);

        // API may return paged or array
        if (Array.isArray(data)) {
          this.expenses = data;
          this.totalCount = data.length;
          this.totalPages = 1;
        } else {
          this.expenses = data.items ?? data;
          this.totalCount = data.totalCount ?? this.expenses.length;
          this.pageNumber = data.pageNumber ?? 1;
          this.totalPages = data.totalPages ?? 1;
        }
      } finally {
        this.loading = false;
      }
    },

    async fetchExpense(id: string) {
      this.loading = true;
      try {
        const { data } = await api.get<ExpenseDto>(`/expenses/${id}`);
        this.currentExpense = data;
        return data;
      } finally {
        this.loading = false;
      }
    },

    async createExpense(payload: CreateExpenseRequest): Promise<ExpenseDto> {
      const { data } = await api.post<ExpenseDto>('/expenses', payload);
      await this.fetchExpenses();
      return data;
    },

    async submitExpense(id: string) {
      await api.put(`/expenses/${id}/submit`);
      await this.fetchExpenses();
    },

    async approveExpense(id: string) {
      await api.put(`/expenses/${id}/approve`);
      await this.fetchExpenses();
    },

    async rejectExpense(id: string, payload: RejectExpenseRequest) {
      await api.put(`/expenses/${id}/reject`, payload);
      await this.fetchExpenses();
    },

    async deleteExpense(id: string) {
      await api.delete(`/expenses/${id}`);
      await this.fetchExpenses();
    },
  },
});
