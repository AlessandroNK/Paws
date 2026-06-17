import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { createBrowserRouter, RouterProvider } from "react-router-dom";
import App from './App.tsx'
import Calendar from "./pages/Calendar.tsx";

const router = createBrowserRouter([
    {path:"/", element: <App />},
    {path:"/calendar", element: <Calendar />},
]);

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <RouterProvider router={router} />
  </StrictMode>,
)
