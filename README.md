# FLAVORIA - Fine Dining Food Ordering & Table Reservation System

A modern, responsive ASP.NET Core web application for luxury fine-dining food ordering and table reservations.

---

## 🌟 Key Features

* **Customer Web Portal**:
  * Luxury warm gold design theme (`#FAF7F2`, `#C59D5F`, `#1A1816`).
  * Chef's Recommendations with product image cards, category tabs, dynamic descriptions, ratings, prices in **MMK**, and direct "Add to Cart".
  * Food detail view with dynamic portion size pricing (Standard 500g, Half 350g, Chef Tasting 750g), ingredients, and real-time total payment calculations.
  * Database-backed persistent cart per logged-in customer.

* **Authentication & User Experience**:
  * Split-screen login and sign-up design matching luxury restaurant aesthetics.
  * Confirmation dialog on user logout.
  * Clean top navigation bar (Streamlined without clutter).

* **5-Step Table Reservation Wizard**:
  1. **Contact Info**: Full Name, Email, and Phone Number validation.
  2. **Date & Time Slot**: Interactive day selector pills, time slot grid chips, and table availability status (🟢 Available vs 🔴 Booked).
  3. **Bank Details**: KBZPay / WavePay mobile wallet options with copyable phone number and expandable QR code.
  4. **Payment Verification**: Receipt screenshot upload (`.png` / `.jpg`) with terms confirmation.
  5. **Outcome Confirmation**: Receipt status and clear next-step guidance.

* **Payment & Currency**:
  * Currency: All food items, totals, and receipts formatted in **MMK** (Myanmar Kyat).
  * Manual payment verification via KBZPay / WavePay screenshot upload.

---

## 🚀 Quick Start

### Prerequisites
* [.NET 10 SDK](https://dotnet.microsoft.com/)

### Running Locally
1. Clone the repository:
   ```bash
   git clone https://github.com/chit-hmue-than-thar/FineDineIn-Food-Ordering-Table-Reservation-System.git
   cd FineDineIn-Food-Ordering-Table-Reservation-System
   ```

2. Build and run the project:
   ```bash
   dotnet run --project src/FoodOrdering.CustomerWeb/FoodOrdering.CustomerWeb.csproj
   ```

3. Open your browser and navigate to:
   `http://localhost:5000` or `https://localhost:7060`

---

## 🔑 Demo Account Credentials

* **Customer Account**:
  * **Email**: `customer@finedinein.com`
  * **Password**: `Password123!`

* **Admin Account**:
  * **Email**: `admin@finedinein.com`
  * **Password**: `Password123!`

---

## 💳 Payment Account Details

* **KBZPay / WavePay Phone**: `09-791234567`
* **Account Name**: `FLAVORIA Fine Dining`
* **Reservation Deposit**: `50,000 MMK / Table`
