import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { createBrowserRouter, RouterProvider } from "react-router-dom";
import App from './App.tsx'
import Calendar from "./pages/Calendar.tsx";
import Profile from "./pages/Profile.tsx";
import AddPet from "./pages/AddPet.tsx";

const router = createBrowserRouter([
    {path:"/", element: <App />},
    {path:"/calendar", element: <Calendar />},
    {path:"/profile", element: <Profile />},
    {path:"/add-pet", element: <AddPet />},
]);

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <RouterProvider router={router} />
  </StrictMode>,
)
