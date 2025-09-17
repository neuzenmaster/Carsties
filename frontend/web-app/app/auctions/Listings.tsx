"use client";

import React, { useEffect, useState } from "react";
import AuctionCard from "./AuctionCard";
import AppPagination from "../components/AppPagination";
import { getData } from "../actions/auctionAction";
import { Auction } from "@/types";
import Filters from "./Filters";
import { useParamsStore } from "@/hooks/useParamsStore";
import { useShallow } from "zustand/shallow";
import queryString from "query-string";
import EmptyFilter from "../components/EmptyFilter";

export default function Listings() {
  const [data, setData] = useState<Auction[]>([]);
  const params = useParamsStore(
    useShallow((state) => ({
      pageNumber: state.pageNumber,
      pageSize: state.pageSize,
      pageCount: state.pageCount,
      searchTerm: state.searchTerm,
      orderBy: state.orderBy,
      filterBy: state.filterBy,
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
      setParams({ pageCount: data.pageCount });
      setData(data.results);
    });
  }, [url, setParams]);

  if (!data) return <h3>Loading...</h3>;

  return (
    <>
      <Filters />
      {pageCount === 0 ? (
        <EmptyFilter />
      ) : (
        <>
          <div className="grid grid-cols-4 gap-6">
            {data &&
              data.map((auction) => (
                <AuctionCard key={auction.id} auction={auction} />
              ))}
          </div>
          <div className="flex justify-center mt-4">
            {pageCount > 0 && (
              <AppPagination
                pageChanged={setPageNumber}
                currentPage={params.pageNumber}
                pageCount={params.pageCount}
              />
            )}
          </div>
        </>
      )}
    </>
  );
}
