
import axios from "axios";

const api = axios.create({
    baseURL: import.meta.env.VITE_API_BASE_URL,
    headers: {
        "Content-Type": "application/json",
    },
});

export type Ticket = {
    id: number;
    name: string;
    price: number;
    totalQuota: number;
    availableQuota: number;
};

export type Order = {
    id: number;
    ticketId: number;
    ticketName: string;
    quantity: number;
    unitPrice: number;
    totalAmount: number;
    status: string;
    createdAt: string;
};

export type Payment = {
    transactionId: number;
    orderId: number;
    amount: number;
    paymentMethod: number | string;
    status: number | string;
    createdAt: string;
    completedAt: string | null;
};

export async function getTickets(): Promise<Ticket[]> {
    const response = await api.get<Ticket[]>("/Tickets");
    return response.data;
}

export async function createOrder(
    ticketId: number,
    quantity: number,
): Promise<Order> {
    const response = await api.post<Order>("/Orders", {
        ticketId,
        quantity,
    });

    return response.data;
}

export async function checkout(
    orderId: number,
    paymentMethod: number,
): Promise<Payment> {
    const response = await api.post<Payment>("/Payments/checkout", {
        orderId,
        paymentMethod,
    });

    return response.data;
}

export async function getPayment(
    transactionId: number,
): Promise<Payment> {
    const response = await api.get<Payment>(
        `/Payments/${transactionId}`,
    );

    return response.data;
}

export default api;
