import Logo from "./Logo.tsx";

function LoadingScreen() {
    return (
        <div className="fill-main flex column gap-3 items-center justify-center h-screen w-screen bg-white">
            <Logo className="h-16 w-16"/>
            <div className="loader"></div>
        </div>
    );
}

export default LoadingScreen;