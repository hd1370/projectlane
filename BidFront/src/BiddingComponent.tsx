import React, { useState, useEffect } from 'react';
import * as signalR from '@microsoft/signalr';
import { 
  Container, 
  Paper, 
  Typography, 
  Box, 
  TextField, 
  Button, 
  List, 
  ListItem 
} from '@mui/material';
import StopAuctionButton from './StopAuctionButton';

interface Bid {
  auctionId: string;
  userId: string;
  amount: string;
}

const BiddingComponent: React.FC = () => {
  const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
  const [bids, setBids] = useState<Bid[]>([]);
  const [auctionId, setAuctionId] = useState<string>('');
  const [userId, setUserId] = useState<string>('');
  const [amount, setAmount] = useState<string>('');
  const [auctionFinished, setAuctionFinished] = useState<boolean>(false);

  const handleAuctionFinished = () => {
    setAuctionFinished(true);
  };


  useEffect(() => {
    const newConnection = new signalR.HubConnectionBuilder()
    .configureLogging(signalR.LogLevel.Debug)
    .withAutomaticReconnect()
    .withUrl('https://localhost:7069/auctionHub',{      
        skipNegotiation: true, 
         transport: signalR.HttpTransportType.WebSockets
    })
    .build();
    setConnection(newConnection);
  }, []);

  useEffect(() => {
    if (connection) {
      connection.start()
        .then(() => {
          connection.on('ReceiveBid', (auctionId: string, userId: string, amount: string) => {
            setBids(prevBids => [...prevBids, { auctionId, userId, amount }]);
          });
        })
        .catch(error => console.error('SignalR Connection Error: ', error));
    }
  }, [connection]);

  const sendBid = async (event: React.FormEvent) => {
    event.preventDefault();
    if (connection) {
      try {
        await connection.invoke('PlaceBid', parseInt(auctionId), parseInt(userId),parseFloat( amount));  
        setAmount('');
      } catch (error) {
        console.error('Error sending bid: ', error);
      }
    }
  };

  return (
    <>
    <Container maxWidth="sm" sx={{ mt: 4 }}>
      <Paper elevation={3} sx={{ p: 3 }}>
        <Typography variant="h4" align="center" gutterBottom>
          Place Your Bid
        </Typography>
        <Box component="form" onSubmit={sendBid} noValidate>
          <TextField
            label="Auction ID"
            variant="outlined"
            fullWidth
            required
            value={auctionId}
            onChange={(e) => setAuctionId(e.target.value)}
            sx={{ mb: 2 }}
          />
          <TextField
            label="User ID"
            variant="outlined"
            fullWidth
            required
            value={userId}
            onChange={(e) => setUserId(e.target.value)}
            sx={{ mb: 2 }}
          />
          <TextField
            label="Amount"
            variant="outlined"
            fullWidth
            required
            type="number"
            value={amount}
            onChange={(e) => setAmount(e.target.value)}
            sx={{ mb: 2 }}
          />
          <Button type="submit" variant="contained" fullWidth>
            Submit Bid
          </Button>
        </Box>
      </Paper>
      <Paper elevation={3} sx={{ mt: 4, p: 3, maxHeight: '400px', overflowY: 'auto'  }}>
        <Typography variant="h5" gutterBottom>
          Live Bids
        </Typography>
        <List>
          {bids.map((bid, index) => (
            <ListItem key={index}>
              Auction {bid.auctionId} - {bid.userId} bid {bid.amount}
            </ListItem>
          ))}
        </List>
      </Paper>
    </Container>
    
    <Container sx={{mt: 4}}>
    <Paper elevation={3} sx={{  mt: 4, p: 3 }}>
      <Typography variant="h4">Admin options</Typography>
      <StopAuctionButton auctionId={1} onAuctionFinished={handleAuctionFinished} />
      {auctionFinished && (
        <Typography variant="h6" color="primary">
          Auction has been finished!
        </Typography>
      )}
      </Paper>
    </Container>
    </>
  );
};

export default BiddingComponent;
