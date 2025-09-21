"use client";

import { useAuctionsStore } from "@/hooks/useAuctionsStore";
import { useBidStore } from "@/hooks/useBidStore";
import { Auction, AuctionFinished, Bid } from "@/types";
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { useParams } from "next/navigation";
import React, { useCallback, useEffect, useRef } from "react";
import toast from "react-hot-toast";
import AuctionCreatedToast from "../components/AuctionCreatedToast";
import { getDetailedViewsData } from "../actions/auctionAction";
import AuctionFinishedToast from "../components/AuctionFinishedToast";
import { useSession } from "next-auth/react";

type Props = {
  children: React.ReactNode;
};

export default function SignalRProvider({ children }: Props) {
  const session = useSession();
  const user = session.data?.user;
  const connection = useRef<HubConnection | null>(null);
  const setCurrentPrice = useAuctionsStore((state) => state.setCurrentPrice);
  const addBid = useBidStore((state) => state.addBid);
  const params = useParams<{ id: string }>();

  const hanldeAuctionFinished = useCallback(
    (finishedAuction: AuctionFinished) => {
      const auction = getDetailedViewsData(finishedAuction.auctionId);
      return toast.promise(
        auction,
        {
          loading: "Loading",
          success: (auction) => (
            <AuctionFinishedToast
              auction={auction}
              finishedAuction={finishedAuction}
            />
          ),
          error: () => "Auction finished",
        },
        { success: { duration: 10000, icon: null } }
      );
    },
    []
  );

  const hanldeAuctionCreate = useCallback(
    (auction: Auction) => {
      if (user?.username !== auction.seller) {
        return toast(<AuctionCreatedToast auction={auction} />, {
          duration: 10000,
        });
      }
    },
    [user?.username]
  );

  const hanldeBidPlaced = useCallback(
    (bid: Bid) => {
      if (bid.bidStatus.includes("Accepted")) {
        setCurrentPrice(bid.auctionId, bid.amount);
      }

      if (params.id === bid.auctionId) {
        addBid(bid);
      }
    },
    [setCurrentPrice, addBid, params.id]
  );

  useEffect(() => {
    if (!connection.current) {
      connection.current = new HubConnectionBuilder()
        .withUrl(process.env.NEXT_PUBLIC_NOTIFY_URL!)
        .withAutomaticReconnect()
        .build();

      connection.current
        .start()
        .then(() => console.log("Connected to notitifications hub"))
        .catch((err) => console.log(err));
    }

    connection.current.on("BidPlaced", hanldeBidPlaced);
    connection.current.on("AuctionCreated", hanldeAuctionCreate);
    connection.current.on("AuctionFinished", hanldeAuctionFinished);

    return () => {
      connection.current?.off("BidPlaced", hanldeBidPlaced);
      connection.current?.off("AuctionCreated", hanldeAuctionCreate);
      connection.current?.off("AuctionFinished", hanldeAuctionFinished);
    };
  }, [
    setCurrentPrice,
    hanldeBidPlaced,
    hanldeAuctionCreate,
    hanldeAuctionFinished,
  ]);

  return children;
}
