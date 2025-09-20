import { Auction, PagedResult } from "@/types";
import { create } from "zustand";

type State = {
  auctions: Auction[];
  totalCount: number;
  pageCount: number;
};

type Action = {
  setData: (data: PagedResult<Auction>) => void;
  setCurrentPrice: (auctionId: string, amount: number) => void;
};

const initialState: State = {
  auctions: [],
  totalCount: 0,
  pageCount: 0,
};

export const useAuctionsStore = create<State & Action>((set) => ({
  ...initialState,

  setData(data: PagedResult<Auction>) {
    set(() => {
      return {
        auctions: data.results,
        pageCount: data.pageCount,
        totalCount: data.totalCount,
      };
    });
  },

  setCurrentPrice(auctionId, amount) {
    set((state) => {
      return {
        ...state,
        auctions: state.auctions.map((auction) =>
          auction.id === auctionId
            ? { ...auction, currentHighBid: amount }
            : auction
        ),
      };
    });
  },
}));
