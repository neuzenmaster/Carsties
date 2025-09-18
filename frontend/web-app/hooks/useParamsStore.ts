import { create } from "zustand";

type State = {
  pageNumber: number;
  pageSize: number;
  pageCount: number;
  searchTerm: string;
  orderBy: string;
  filterBy: string;
  seller?: string;
  winner?: string;
};

type Action = {
  setParams: (params: Partial<State>) => void;
  reset: () => void;
};

const initialState: State = {
  pageNumber: 1,
  pageSize: 4,
  pageCount: 1,
  searchTerm: "",
  orderBy: "make",
  filterBy: "live",
  seller: undefined,
  winner: undefined,
};

export const useParamsStore = create<State & Action>((set) => ({
  ...initialState,

  setParams: (newParams: Partial<State>) => {
    set((state) => {
      if (newParams.pageNumber) {
        return { ...state, pageNumber: newParams.pageNumber };
      } else {
        return { ...state, ...newParams };
      }
    });
  },

  reset: () => set(initialState),
}));
