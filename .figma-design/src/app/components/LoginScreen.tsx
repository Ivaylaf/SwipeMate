import React, { useState } from 'react';
import { Button } from './ui/button';
import { Input } from './ui/input';
import { Card } from './ui/card';
import { useApp } from '../context/AppContext';

interface LoginScreenProps {
  onSwitchToRegister: () => void;
}

// Custom stacked cards logo
const SwipeMateIcon = () => (
  <svg width="48" height="48" viewBox="0 0 48 48" fill="none" xmlns="http://www.w3.org/2000/svg">
    {/* Back card */}
    <rect
      x="16"
      y="8"
      width="20"
      height="28"
      rx="3"
      fill="white"
      opacity="0.4"
      transform="rotate(-8 26 22)"
    />
    {/* Middle card */}
    <rect
      x="14"
      y="10"
      width="20"
      height="28"
      rx="3"
      fill="white"
      opacity="0.7"
      transform="rotate(-4 24 24)"
    />
    {/* Front card */}
    <rect
      x="12"
      y="12"
      width="20"
      height="28"
      rx="3"
      fill="white"
    />
    {/* Swipe arrow indicator */}
    <path
      d="M 34 26 L 40 26 M 37 23 L 40 26 L 37 29"
      stroke="white"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
    />
  </svg>
);

export const LoginScreen: React.FC<LoginScreenProps> = ({ onSwitchToRegister }) => {
  const { login } = useApp();
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');

  const handleLogin = (e: React.FormEvent) => {
    e.preventDefault();
    if (username && password) {
      login(username, password);
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-pink-50 to-purple-50 flex items-center justify-center p-4">
      <Card className="w-full max-w-md p-8 space-y-6">
        <div className="text-center space-y-2">
          <div className="flex justify-center mb-4">
            <div className="bg-gradient-to-br from-pink-500 to-purple-500 p-4 rounded-full">
              <SwipeMateIcon />
            </div>
          </div>
          <h1 className="text-3xl font-bold bg-gradient-to-r from-pink-500 to-purple-500 bg-clip-text text-transparent">
            SwipeMate
          </h1>
          <p className="text-gray-600">Make decisions together</p>
        </div>

        <form onSubmit={handleLogin} className="space-y-4">
          <div className="space-y-2">
            <label htmlFor="username" className="text-sm font-medium">
              Username
            </label>
            <Input
              id="username"
              type="text"
              placeholder="Enter your username"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              required
            />
          </div>

          <div className="space-y-2">
            <label htmlFor="password" className="text-sm font-medium">
              Password
            </label>
            <Input
              id="password"
              type="password"
              placeholder="Enter your password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </div>

          <Button type="submit" className="w-full bg-gradient-to-r from-pink-500 to-purple-500">
            Log In
          </Button>
        </form>

        <div className="text-center">
          <button
            onClick={onSwitchToRegister}
            className="text-sm text-purple-600 hover:text-purple-700 hover:underline"
          >
            Don't have an account? Sign up
          </button>
        </div>
      </Card>
    </div>
  );
};