
import { useEffect, useState } from "react";
import {
    checkout,
    createOrder,
    getPayment,
    getTickets,
    type Payment,
    type Ticket,
} from "./api/api";
import "./App.css";

function isSucceeded(status: Payment["status"]) {
    return status === 2 || String(status).toLowerCase() === "succeeded";
}

function isPending(status: Payment["status"]) {
    return status === 1 || String(status).toLowerCase() === "pending";
}

function App() {
    const [tickets, setTickets] = useState<Ticket[]>([]);
    const [selectedId, setSelectedId] = useState<number | null>(null);
    const [quantity, setQuantity] = useState(1);
    const [paymentMethod, setPaymentMethod] = useState(1);
    const [payment, setPayment] = useState<Payment | null>(null);
    const [loadingTickets, setLoadingTickets] = useState(true);
    const [processing, setProcessing] = useState(false);
    const [error, setError] = useState("");
    const [notice, setNotice] = useState("");

    const selectedTicket =
        tickets.find((ticket) => ticket.id === selectedId) ?? null;

    useEffect(() => {
        async function loadTickets() {
            try {
                const data = await getTickets();
                const availableTickets = data.filter(
                    (ticket) => ticket.availableQuota > 0,
                );

                setTickets(availableTickets);
                setSelectedId(availableTickets[0]?.id ?? null);
            } catch {
                setError(
                    "Could not load tickets. Check that the backend is running and the API URL is correct.",
                );
            } finally {
                setLoadingTickets(false);
            }
        }

        void loadTickets();
    }, []);

    useEffect(() => {
        if (!payment || !isPending(payment.status)) return;

        let cancelled = false;
        let timer: ReturnType<typeof setTimeout> | undefined;

        async function pollPayment() {
            try {
                const latest = await getPayment(payment!.transactionId);

                if (cancelled) return;

                setPayment(latest);

                if (isPending(latest.status)) {
                    timer = setTimeout(() => void pollPayment(), 1500);
                } else if (!isSucceeded(latest.status)) {
                    setNotice("The payment did not complete successfully.");
                }
            } catch {
                if (!cancelled) {
                    timer = setTimeout(() => void pollPayment(), 2000);
                }
            }
        }

        timer = setTimeout(() => void pollPayment(), 1500);

        return () => {
            cancelled = true;
            if (timer) clearTimeout(timer);
        };
    }, [payment?.transactionId, payment?.status]);

    async function handleCheckout() {
        if (!selectedTicket) return;

        setProcessing(true);
        setError("");
        setNotice("");
        setPayment(null);

        try {
            const order = await createOrder(selectedTicket.id, quantity);
            const result = await checkout(order.id, paymentMethod);
            setPayment(result);

            if (isPending(result.status)) {
                setNotice("Payment is pending. Waiting for confirmation...");
            } else if (!isSucceeded(result.status)) {
                setNotice("Payment was not successful.");
            }
        } catch (err: unknown) {
            if (
                typeof err === "object" &&
                err !== null &&
                "response" in err
            ) {
                const response = (
                    err as {
                        response?: { data?: { message?: string; title?: string } };
                    }
                ).response;

                setError(
                    response?.data?.message ??
                    response?.data?.title ??
                    "Checkout failed. Please try again.",
                );
            } else {
                setError("Could not connect to the API. Check your backend URL.");
            }
        } finally {
            setProcessing(false);
        }
    }

    function resetBooking() {
        setPayment(null);
        setNotice("");
        setError("");
        setQuantity(1);
    }

    const success = payment && isSucceeded(payment.status);

    if (loadingTickets) {
        return <main className="container"><p>Loading tickets...</p></main>;
    }

    if (success) {
        return (
            <main className="container">
                <section className="checkout-panel success-panel">
                    <div className="success-icon">✓</div>
                    <span className="eyebrow">PAYMENT CONFIRMED</span>
                    <h1>Payment Successful!</h1>
                    <p>Your booking payment has been completed.</p>

                    <div className="transaction-details">
                        <span>Transaction ID</span>
                        <strong>{payment.transactionId}</strong>

                        <span>Timestamp</span>
                        <strong>
                            {new Date(
                                payment.completedAt ?? payment.createdAt,
                            ).toLocaleString()}
                        </strong>

                        <span>Amount paid</span>
                        <strong>AED {payment.amount.toFixed(2)}</strong>
                    </div>

                    <button className="checkout-button" onClick={resetBooking}>
                        Book another ticket
                    </button>
                </section>
            </main>
        );
    }

    return (
        <div className="app">
            <header className="navbar">
                <a className="brand" href="/">
                    <span className="brand-icon">E</span>
                    E-Ticketing
                </a>
                <span className="nav-label">Event Tickets</span>
            </header>

            <main className="container">
                <section className="intro">
                    <span className="eyebrow">YOUR NEXT EXPERIENCE</span>
                    <h1>Find your perfect ticket.</h1>
                    <p>Choose your ticket, select a payment method, and book securely.</p>
                </section>

                {error && <div className="error-message">{error}</div>}

                {tickets.length === 0 ? (
                    <p>No tickets are currently available.</p>
                ) : (
                    <>
                        <section className="ticket-grid">
                            {tickets.map((ticket) => (
                                <button
                                    key={ticket.id}
                                    type="button"
                                    className={`ticket-card ${selectedId === ticket.id ? "selected" : ""} ${ticket.name.toLowerCase()}`}
                                    disabled={processing}
                                    onClick={() => {
                                        setSelectedId(ticket.id);
                                        setQuantity(1);
                                    }}
                                >
                                    <div className="ticket-top">
                                        <span className="ticket-label">ADMISSION</span>
                                        <span className="ticket-symbol">
                                            {ticket.name.charAt(0)}
                                        </span>
                                    </div>

                                    <h2>{ticket.name}</h2>
                                    <div className="ticket-price">
                                        <span>AED</span>
                                        <strong>{ticket.price}</strong>
                                    </div>

                                    <div className="ticket-bottom">
                                        <span>{ticket.availableQuota} available</span>
                                        <span>
                                            {selectedId === ticket.id ? "✓ Selected" : "Select →"}
                                        </span>
                                    </div>
                                </button>
                            ))}
                        </section>

                        <section className="checkout-panel">
                            <div className="checkout-heading">
                                <div>
                                    <span className="eyebrow">YOUR ORDER</span>
                                    <h2>Complete your booking</h2>
                                </div>
                                <span className="secure-label">Secure checkout</span>
                            </div>

                            <div className="checkout-details">
                                <div className="quantity-control">
                                    <label htmlFor="quantity">Quantity</label>
                                    <div className="quantity-buttons">
                                        <button
                                            type="button"
                                            disabled={quantity <= 1 || processing}
                                            onClick={() => setQuantity((q) => q - 1)}
                                        >
                                            −
                                        </button>
                                        <span>{quantity}</span>
                                        <button
                                            type="button"
                                            disabled={
                                                processing ||
                                                quantity >= 10 ||
                                                quantity >= selectedTicket!.availableQuota
                                            }
                                            onClick={() => setQuantity((q) => q + 1)}
                                        >
                                            +
                                        </button>
                                    </div>
                                </div>

                                <div className="order-total">
                                    <span>Total amount</span>
                                    <strong>
                                        AED {(selectedTicket!.price * quantity).toFixed(2)}
                                    </strong>
                                </div>
                            </div>

                            <label htmlFor="paymentMethod">Payment method</label>
                            <select
                                id="paymentMethod"
                                className="payment-select"
                                value={paymentMethod}
                                disabled={processing}
                                onChange={(event) =>
                                    setPaymentMethod(Number(event.target.value))
                                }
                            >
                                <option value={1}>Credit Card</option>
                                <option value={2}>QR Payment</option>
                            </select>

                            {notice && <div className="notice-message">{notice}</div>}

                            {payment && isPending(payment.status) && (
                                <p>
                                    Transaction #{payment.transactionId} is awaiting confirmation.
                                </p>
                            )}

                            <button
                                className="checkout-button"
                                disabled={processing || !selectedTicket}
                                onClick={() => void handleCheckout()}
                            >
                                {processing ? "Processing..." : "Checkout and Pay →"}
                            </button>

                            <p className="checkout-note">
                                The server validates availability and calculates the order total.
                            </p>
                        </section>
                    </>
                )}
            </main>
        </div>
    );
}

export default App;
