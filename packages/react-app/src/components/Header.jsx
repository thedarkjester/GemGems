import React, { useState } from "react";
import { Helmet } from "react-helmet";
import { BrowserRouter as Router, Link } from "react-router-dom";
import Account from "./Account";
export default function Header(props) {
  const {
    address,
    localProvider,
    userSigner,
    mainnetProvider,
    price,
    web3Modal,
    loadWeb3Modal,
    logoutOfWeb3Modal,
    blockExplorer,
    gas,
  } = props;
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

  const accountInfo = (
    <div className="flex flex-row">
      <Account
        address={address}
        localProvider={localProvider}
        userSigner={userSigner}
        mainnetProvider={mainnetProvider}
        price={price}
        web3Modal={web3Modal}
        loadWeb3Modal={loadWeb3Modal}
        logoutOfWeb3Modal={logoutOfWeb3Modal}
        blockExplorer={blockExplorer}
      />
      <span className="mt-3 ml-3" role="img" aria-label="fuelpump">
        ⛽️ {parseInt(gas, 10) / 10 ** 9}g
      </span>
    </div>
  );
  const menuItems = (
    <Router>
      <Link to="/">
        <div
          onClick={() => setMobileMenuOpen(false)}
          className="block md:inline-block px-4 py-2 md:mr-2 lg:mr-8 hover:text-purple-500"
        >
          My Gems
        </div>
      </Link>

      <Link to="/mint">
        <div
          onClick={() => setMobileMenuOpen(false)}
          className="block md:inline-block px-4 py-2 md:px-0 md:py-0 md:mr-2 lg:mr-8 hover:text-purple-500"
        >
          Mint
        </div>
      </Link>

      <Link to="/how-it-works">
        <div
          onClick={() => setMobileMenuOpen(false)}
          className="block md:inline-block px-4 py-2 md:px-0 md:py-0 md:mr-2 lg:mr-8 hover:text-purple-500"
        >
          How it works
        </div>
      </Link>

      <Link to="/placeholder">
        <div
          onClick={() => setMobileMenuOpen(false)}
          className="block md:inline-block px-4 py-2 mx-2 md:mx-0 md:ml-2 font-bold text-white bg-purple-600 rounded-full hover:bg-purple-500"
        >
          Placeholder
        </div>
      </Link>
    </Router>
  );

  const mobileMenu = (
    <nav className="md:hidden fixed right-0 bottom-0 top-12 bg-white w-3/4 shadow-lg">{menuItems}</nav>
  );

  return (
    <>
      <Helmet>
        <meta charSet="utf-8" />
        <title>GemGems</title>
        <link rel="canonical" href="https://gemgems.io" />
      </Helmet>

      <div id="header" className="w-full fixed bg-white z-10 py-1 shadow-md">
        <>
          <div className="container mx-auto flex justify-between items-center px-4 py-1 lg:py-0">
            <a href="/">
              <img src="/gemgems_logo.svg" style={{ height: "50px" }} />
            </a>

            <button className="md:hidden" onClick={() => setMobileMenuOpen(!mobileMenuOpen)}>
              <svg
                xmlns="http://www.w3.org/2000/svg"
                className="h-6 w-6"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
              >
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M4 8h16M4 16h16" />
              </svg>
            </button>

            {mobileMenuOpen ? mobileMenu : ""}
            {accountInfo}
            <div className="hidden md:flex items-center m-2 sm:m-4" id="links">
              {menuItems}
            </div>
          </div>
        </>
      </div>
    </>
  );
}
