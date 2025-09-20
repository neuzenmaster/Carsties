"use client";

import React, { useEffect, useState } from "react";
import AuctionCard from "./AuctionCard";
import AppPagination from "../components/AppPagination";
import { getData } from "../actions/auctionAction";
import Filters from "./Filters";
import { useParamsStore } from "@/hooks/useParamsStore";
import { useShallow } from "zustand/shallow";
import queryString from "query-string";
import EmptyFilter from "../components/EmptyFilter";
import { useAuctionsStore } from "@/hooks/useAuctionsStore";

export default function Listings() {
  const [loading, setLoading] = useState(true);

  const data = useAuctionsStore(
    useShallow((state) => ({
      auctions: state.auctions,
      totalCount: state.pageCount,
      pageCount: state.pageCount,
    }))
  );

  const setData = useAuctionsStore((state) => state.setData);

  const params = useParamsStore(
    useShallow((state) => ({
      pageNumber: state.pageNumber,
      pageSize: state.pageSize,
      pageCount: state.pageCount,
      searchTerm: state.searchTerm,
      orderBy: state.orderBy,
      filterBy: state.filterBy,
      seller: state.seller,
      winner: state.winner,
    }))
  );
  const setParams = useParamsStore((state) => state.setParams);
  const pageCount = useParamsStore((state) => state.pageCount);
  const url = queryString.stringifyUrl(
    { url: "", query: params },
    { skipEmptyString: true }
  );

  function setPageNumber(pageNumber: number) {
    setParams({ pageNumber });
  }

  useEffect(() => {
    getData(url).then((data) => {
      setData(data);
      setLoading(false);
    });
  }, [url, setData]);

  if (loading) return <h3>Loading...</h3>;

  return (
    <>
      <Filters />
      {pageCount === 0 ? (
        <EmptyFilter />
      ) : (
        <>
          <div className="grid grid-cols-4 gap-6">
            {data &&
              data.auctions.map((auction) => (
                <AuctionCard key={auction.id} auction={auction} />
              ))}
          </div>
          <div className="flex justify-center mt-4">
            {pageCount > 0 && (
              <AppPagination
                pageChanged={setPageNumber}
                currentPage={params.pageNumber}
                pageCount={data.pageCount}
              />
            )}
          </div>
        </>
      )}
    </>
  );
}
