process.env.NODE_TLS_REJECT_UNAUTHORIZED = "0";

import * as signalR from "@microsoft/signalr";
import WebSocket from "ws";
import fetch from "node-fetch";

globalThis.fetch = fetch;
globalThis.WebSocket = WebSocket;

function sleep(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

async function runClient(clientId) {
  const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:7069/auctionHub", {
      transport: signalR.HttpTransportType.WebSockets,
    })
    .configureLogging(signalR.LogLevel.Error)
    .build();

  connection.on("ReceiveBid", (auctionId, userId, amount) => {
    // console.log(
    //   `[Client ${clientId}] Received bid: auction ${auctionId}, user ${userId}, amount ${amount}`
    // );
  });

  try {
    await connection.start();
    console.log(`[Client ${clientId}] Connected to SignalR hub.`);
  } catch (err) {
    console.error(`[Client ${clientId}] Connection error:`, err);
    return;
  }

  // Simulate bidding for given MS
  const startTime = Date.now();
  while (Date.now() - startTime < 60000) {
    const randomAmount = (Math.random() * 1000 + 1).toFixed(2);
    try {
        // Hardcoded min and max values for existing users and bids in local database.
      await connection.invoke("PlaceBid", Math.floor(Math.random() * (11 - 1 + 1)) + 1,  Math.floor(Math.random() * (20 - 11 + 1)) + 11, parseFloat(randomAmount));
      console.log(`[Client ${clientId}] Sent bid: ${randomAmount}`);
    } catch (err) {
      console.error(`[Client ${clientId}] Error sending bid:`, err);
    }
    await sleep(3000); // Chose delay in MS
  }

  try {
    await connection.stop();
  } catch (err) {
    console.error(`[Client ${clientId}] Error during disconnection:`, err);
  }
}

async function runLoadTest(numClients) {
  console.log(`Starting load test with ${numClients} clients for 1 minute...`);
  const clients = [];
  for (let i = 1; i <= numClients; i++) {
    clients.push(runClient(i));
  }
  await Promise.all(clients);
  console.log("Load test completed.");

  // Finalize the auction by calling the finalization endpoint 10 times (amount of auctions in local database)
  for (let i = 1; i <= 10; i++) {
  try {
    let finalRes = await fetch(`https://localhost:7069/api/auctions/${i}/finish`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
    });
    if (finalRes.ok) {
      let result = await finalRes.text();
      console.log("Auction finalized:", result);
    } else {
      console.error("Auction finalization failed with status:", finalRes.status);
    }
  } catch (err) {
    console.error("Error during auction finalization:", err);
  }
}
}

// Run load test with given amount of clients.
runLoadTest(50).catch((err) => console.error("Error during load test:", err));
