import http from 'k6/http';
import { sleep, check } from 'k6';

export let options = {
  vus: 100,
  duration: '1m',
};

export default function () {
  let randomAmount = (Math.random() * 10000 + 1).toFixed(2);
  let auctionId = 1; // random valid auction
  let userId = Math.floor(Math.random() * 100) + 1; // random invalid userid, no valdiation yet
  let payload = JSON.stringify({
    auctionId: auctionId,
    userId: userId,
    amount: parseFloat(randomAmount),
    timestamp: new Date().toISOString()
  });
  let params = { headers: { "Content-Type": "application/json" } };
  let res = http.post("https://localhost:7069/api/bids", payload, params);
  check(res, { "status is 200": (r) => r.status === 200 });
  sleep(0.5);
}