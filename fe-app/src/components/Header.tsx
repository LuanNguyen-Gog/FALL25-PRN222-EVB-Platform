'use client';

import React, { useState } from 'react';
import Link from 'next/link';

const Header: React.FC = () => {
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

  const toggleMobileMenu = () => {
    setIsMobileMenuOpen(!isMobileMenuOpen);
  };

  return (
    <header className="bg-white shadow-md">
      <div className="container mx-auto px-4 py-4 flex justify-between items-center">
        {/* Logo */}
        <Link href="/" className="text-2xl font-bold text-gray-800">
          EVB Platform
        </Link>

        {/* Desktop Navigation */}
        <nav className="hidden md:flex space-x-6">
          <Link href="/" className="text-gray-600 hover:text-blue-600 transition-colors duration-200">
            Trang chủ
          </Link>
          <Link href="/batteries" className="text-gray-600 hover:text-blue-600 transition-colors duration-200">
            Pin xe điện
          </Link>
          <Link href="/vehicles" className="text-gray-600 hover:text-blue-600 transition-colors duration-200">
            Xe điện
          </Link>
          <Link href="/about" className="text-gray-600 hover:text-blue-600 transition-colors duration-200">
            Giới thiệu
          </Link>
          <Link href="/contact" className="text-gray-600 hover:text-blue-600 transition-colors duration-200">
            Liên hệ
          </Link>
        </nav>

        {/* Auth Buttons */}
        <div className="hidden md:flex space-x-4">
          <Link href="/login" className="px-4 py-2 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-100 transition-colors duration-200">
            Đăng nhập
          </Link>
          <Link href="/register" className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors duration-200">
            Đăng ký
          </Link>
        </div>

        {/* Mobile Menu Button */}
        <div className="md:hidden">
          <button onClick={toggleMobileMenu} className="text-gray-600 hover:text-blue-600 focus:outline-none">
            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M4 6h16M4 12h16m-7 6h7"></path>
            </svg>
          </button>
        </div>
      </div>

      {/* Mobile Menu */}
      {isMobileMenuOpen && (
        <div className="md:hidden bg-white border-t border-gray-200">
          <nav className="flex flex-col px-4 py-2 space-y-2">
            <Link href="/" className="block text-gray-600 hover:text-blue-600 transition-colors duration-200 py-1">
              Trang chủ
            </Link>
            <Link href="/batteries" className="block text-gray-600 hover:text-blue-600 transition-colors duration-200 py-1">
              Pin xe điện
            </Link>
            <Link href="/vehicles" className="block text-gray-600 hover:text-blue-600 transition-colors duration-200 py-1">
              Xe điện
            </Link>
            <Link href="/about" className="block text-gray-600 hover:text-blue-600 transition-colors duration-200 py-1">
              Giới thiệu
            </Link>
            <Link href="/contact" className="block text-gray-600 hover:text-blue-600 transition-colors duration-200 py-1">
              Liên hệ
            </Link>
            <div className="border-t border-gray-200 my-2"></div>
            <Link href="/login" className="block px-4 py-2 text-gray-700 hover:bg-gray-100 transition-colors duration-200">
              Đăng nhập
            </Link>
            <Link href="/register" className="block px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors duration-200">
              Đăng ký
            </Link>
          </nav>
        </div>
      )}
    </header>
  );
};

export default Header;
