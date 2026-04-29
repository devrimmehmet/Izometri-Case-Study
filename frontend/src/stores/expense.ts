import { defineStore } from 'pinia';
import { api } from 'src/services/http';
import type {
  ExpenseDto,
  CreateExpenseRequest,
  UpdateExpenseRequest,
  RejectExpenseRequest,
  ExpenseQueryParams,
  PagedResponse,
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

        const { data } = await api.get<PagedResponse<ExpenseDto>>(`/expenses?${query.toString()}`);

        this.expenses = data.items;
        this.totalCount = data.totalCount;
        this.pageNumber = data.pageNumber;
        this.pageSize = data.pageSize;
        this.totalPages = Math.max(1, Math.ceil(data.totalCount / data.pageSize));
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
      return data;
    },

    async updateExpense(id: string, payload: UpdateExpenseRequest): Promise<ExpenseDto> {
      const { data } = await api.put<ExpenseDto>(`/expenses/${id}`, payload);
      return data;
    },

    async submitExpense(id: string) {
      await api.put(`/expenses/${id}/submit`);
    },

    async approveExpense(id: string) {
      await api.put(`/expenses/${id}/approve`);
    },

    async rejectExpense(id: string, payload: RejectExpenseRequest) {
      await api.put(`/expenses/${id}/reject`, payload);
    },

    async deleteExpense(id: string) {
      await api.delete(`/expenses/${id}`);
    },
  },
});
