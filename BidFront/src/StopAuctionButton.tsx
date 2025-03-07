import React from 'react';
import { Button } from '@mui/material';

interface StopAuctionButtonProps {
  auctionId: number;
  onAuctionFinished?: (auction: unknown) => void; // Callback for further processing
}

const StopAuctionButton: React.FC<StopAuctionButtonProps> = ({ auctionId, onAuctionFinished }) => {
  const aucId = parseInt(auctionId);
  const stopAuction = async () => {
    try {
      const response = await fetch(`https://localhost:7069/api/auctions/${aucId}/finish`, {
        method: 'POST'
      });
      if (!response.ok) {
        throw new Error('Failed to finish auction');
      }
      const finishedAuction = await response.json();
      console.log('Auction finished:', finishedAuction);
      // Optionally, update parent component state with finishedAuction
      if (onAuctionFinished) {
        onAuctionFinished(finishedAuction);
      }
    } catch (error) {
      console.error('Error finishing auction:', error);
    }
  };

  return (
    <Button variant="contained" color="secondary" onClick={stopAuction}>
      Stop Auction
    </Button>
  );
};

export default StopAuctionButton;
