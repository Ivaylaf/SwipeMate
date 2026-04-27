import React, { useState } from 'react';
import { AppProvider, useApp } from './context/AppContext';
import { LoginScreen } from './components/LoginScreen';
import { RegisterScreen } from './components/RegisterScreen';
import { HomeScreen } from './components/HomeScreen';
import { FriendsScreen } from './components/FriendsScreen';
import { CreateSessionScreen } from './components/CreateSessionScreen';
import { FiltersScreen } from './components/FiltersScreen';
import { SwipeScreen } from './components/SwipeScreen';
import { MatchResultScreen } from './components/MatchResultScreen';
import { HistoryScreen } from './components/HistoryScreen';
import { ProfileScreen } from './components/ProfileScreen';
import { Category } from './types';
import { Toaster } from './components/ui/sonner';
import { WelcomeTour } from './components/WelcomeTour';

type Screen =
  | 'auth'
  | 'home'
  | 'friends'
  | 'createSession'
  | 'filters'
  | 'swipe'
  | 'matchResult'
  | 'history'
  | 'profile';

const AppContent: React.FC = () => {
  const { isAuthenticated, currentSession } = useApp();
  const [currentScreen, setCurrentScreen] = useState<Screen>('auth');
  const [authMode, setAuthMode] = useState<'login' | 'register'>('login');
  const [selectedCategory, setSelectedCategory] = useState<Category | null>(null);
  const [showWelcomeTour, setShowWelcomeTour] = useState(false);

  // Auto-navigate after authentication
  React.useEffect(() => {
    if (isAuthenticated && currentScreen === 'auth') {
      setCurrentScreen('home');
      setShowWelcomeTour(true);
    }
  }, [isAuthenticated]);

  // Auto-navigate to match result when session completes
  React.useEffect(() => {
    if (currentSession?.status === 'completed' && currentScreen !== 'matchResult') {
      setCurrentScreen('matchResult');
    }
  }, [currentSession?.status]);

  const handleSelectCategory = (category: Category) => {
    setSelectedCategory(category);
    setCurrentScreen('createSession');
  };

  const handleBackToHome = () => {
    setCurrentScreen('home');
    setSelectedCategory(null);
  };

  // Auth screens
  if (!isAuthenticated) {
    if (authMode === 'login') {
      return (
        <LoginScreen onSwitchToRegister={() => setAuthMode('register')} />
      );
    }
    return (
      <RegisterScreen onSwitchToLogin={() => setAuthMode('login')} />
    );
  }

  // Main app screens
  switch (currentScreen) {
    case 'home':
      return (
        <>
          <HomeScreen
            onSelectCategory={handleSelectCategory}
            onNavigateToFriends={() => setCurrentScreen('friends')}
            onNavigateToHistory={() => setCurrentScreen('history')}
            onNavigateToProfile={() => setCurrentScreen('profile')}
            onShowHelp={() => setShowWelcomeTour(true)}
          />
          {showWelcomeTour && <WelcomeTour onClose={() => setShowWelcomeTour(false)} />}
        </>
      );

    case 'friends':
      return <FriendsScreen onBack={handleBackToHome} />;

    case 'createSession':
      return selectedCategory ? (
        <CreateSessionScreen
          category={selectedCategory}
          onBack={handleBackToHome}
          onContinue={() => setCurrentScreen('filters')}
        />
      ) : null;

    case 'filters':
      return (
        <FiltersScreen
          onBack={() => setCurrentScreen('createSession')}
          onStartSwipe={() => setCurrentScreen('swipe')}
        />
      );

    case 'swipe':
      return <SwipeScreen onBack={handleBackToHome} />;

    case 'matchResult':
      return <MatchResultScreen onBackToHome={handleBackToHome} />;

    case 'history':
      return <HistoryScreen onBack={handleBackToHome} />;

    case 'profile':
      return <ProfileScreen onBack={handleBackToHome} />;

    default:
      return (
        <HomeScreen
          onSelectCategory={handleSelectCategory}
          onNavigateToFriends={() => setCurrentScreen('friends')}
          onNavigateToHistory={() => setCurrentScreen('history')}
          onNavigateToProfile={() => setCurrentScreen('profile')}
        />
      );
  }
};

export default function App() {
  return (
    <AppProvider>
      <AppContent />
      <Toaster />
    </AppProvider>
  );
}