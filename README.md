# Personal Finance Tracker

A decoupled personal finance tracker with an ASP.NET Core REST API and a React/Tailwind frontend.

## Run the API

```powershell
cd backend\PersonalFinanceTracker.Api
dotnet run
```

The API listens on `http://localhost:5000`. Data is persisted in `backend/PersonalFinanceTracker.Api/Data/users.json` and `transactions.json`.

## Run the frontend

```powershell
cd frontend
npm install
npm run dev
```

The UI runs on `http://localhost:5173`. Set `VITE_API_URL` if the API uses another URL.

## Public pages

The frontend includes public product pages that do not require authentication:

- `/` — product landing page
- `/features` — product capabilities
- `/money-habits` — public money habits check with personalized suggestions
- `/guides` — beginner-friendly financial guides
- `/security` — authentication, data scope, and demo storage notes
- `/login` and `/register` — authentication
- `/dashboard` — protected transaction workspace

## API

- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/transactions?type=Income&category=Salary&search=monthly`
- `POST /api/transactions`
- `PUT /api/transactions/{id}`
- `POST /api/public/money-habits`

Transaction endpoints require `Authorization: Bearer <token>`.
