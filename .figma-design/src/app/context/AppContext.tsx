import React, { createContext, useContext, useState, ReactNode } from 'react';
import {
  User,
  Friend,
  FriendRequest,
  MatchSession,
  MatchHistory,
  Category,
  ContentItem,
  SwipeAction,
} from '../types';
import {
  mockCurrentUser,
  mockFriends,
  mockFriendRequests,
  mockMatchHistory,
  mockMovies,
  mockRestaurants,
  mockRecipes,
  mockBoardGames,
} from '../data/mockData';

interface AppContextType {
  currentUser: User | null;
  friends: Friend[];
  friendRequests: FriendRequest[];
  matchHistory: MatchHistory[];
  currentSession: MatchSession | null;
  isAuthenticated: boolean;
  login: (username: string, password: string) => void;
  logout: () => void;
  register: (username: string, email: string, password: string) => void;
  acceptFriendRequest: (requestId: string) => void;
  rejectFriendRequest: (requestId: string) => void;
  addFriend: (username: string) => void;
  createSession: (category: Category, friendIds: string[], filters: any) => void;
  joinSession: (sessionId: string) => void;
  recordSwipe: (itemId: string, action: 'like' | 'reject') => void;
  completeSession: (result: ContentItem) => void;
  getContentForCategory: (category: Category) => ContentItem[];
  currentSwipeIndex: number;
  setCurrentSwipeIndex: (index: number) => void;
  sessionSwipes: Map<string, SwipeAction[]>;
}

const AppContext = createContext<AppContextType | undefined>(undefined);

export const AppProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [currentUser, setCurrentUser] = useState<User | null>(null);
  const [friends, setFriends] = useState<Friend[]>(mockFriends);
  const [friendRequests, setFriendRequests] = useState<FriendRequest[]>(mockFriendRequests);
  const [matchHistory, setMatchHistory] = useState<MatchHistory[]>(mockMatchHistory);
  const [currentSession, setCurrentSession] = useState<MatchSession | null>(null);
  const [currentSwipeIndex, setCurrentSwipeIndex] = useState(0);
  const [sessionSwipes, setSessionSwipes] = useState<Map<string, SwipeAction[]>>(new Map());

  const login = (username: string, password: string) => {
    // Mock login - in real app would authenticate with backend
    setCurrentUser(mockCurrentUser);
    setIsAuthenticated(true);
  };

  const logout = () => {
    setCurrentUser(null);
    setIsAuthenticated(false);
    setCurrentSession(null);
  };

  const register = (username: string, email: string, password: string) => {
    // Mock registration
    const newUser: User = {
      id: Math.random().toString(36).substring(7),
      username,
      email,
      reviewCount: 0,
      ratingCount: 0,
      badges: [],
    };
    setCurrentUser(newUser);
    setIsAuthenticated(true);
  };

  const acceptFriendRequest = (requestId: string) => {
    const request = friendRequests.find((r) => r.id === requestId);
    if (request) {
      setFriends([...friends, request.fromUser]);
      setFriendRequests(friendRequests.filter((r) => r.id !== requestId));
    }
  };

  const rejectFriendRequest = (requestId: string) => {
    setFriendRequests(friendRequests.filter((r) => r.id !== requestId));
  };

  const addFriend = (username: string) => {
    // Mock friend request - in real app would send to backend
    console.log(`Friend request sent to ${username}`);
  };

  const createSession = (category: Category, friendIds: string[], filters: any) => {
    const session: MatchSession = {
      id: Math.random().toString(36).substring(7),
      category,
      createdBy: currentUser?.id || '',
      participants: [currentUser?.id || '', ...friendIds],
      filters,
      status: 'active',
      createdAt: new Date(),
    };
    setCurrentSession(session);
    setCurrentSwipeIndex(0);
    setSessionSwipes(new Map());
  };

  const joinSession = (sessionId: string) => {
    // Mock joining session - in real app would connect to backend session
    console.log(`Joined session ${sessionId}`);
  };

  const recordSwipe = (itemId: string, action: 'like' | 'reject') => {
    if (!currentSession || !currentUser) return;

    const swipeAction: SwipeAction = {
      userId: currentUser.id,
      itemId,
      action,
    };

    const sessionId = currentSession.id;
    const currentSwipes = sessionSwipes.get(sessionId) || [];
    const updatedSwipes = [...currentSwipes, swipeAction];
    
    const newSwipesMap = new Map(sessionSwipes);
    newSwipesMap.set(sessionId, updatedSwipes);
    setSessionSwipes(newSwipesMap);

    // Check for matches - if all participants liked the same item
    if (action === 'like') {
      const itemLikes = updatedSwipes.filter(
        (s) => s.itemId === itemId && s.action === 'like'
      );
      
      // For demo purposes, simulate match if user liked it (in real app would check all participants)
      if (itemLikes.length >= 1) {
        const content = getContentForCategory(currentSession.category);
        const matchedItem = content.find((item) => item.id === itemId);
        if (matchedItem) {
          setTimeout(() => {
            completeSession(matchedItem);
          }, 500);
        }
      }
    }
  };

  const completeSession = (result: ContentItem) => {
    if (!currentSession || !currentUser) return;

    const completedSession: MatchSession = {
      ...currentSession,
      status: 'completed',
      result,
    };

    const history: MatchHistory = {
      id: Math.random().toString(36).substring(7),
      session: completedSession,
      result,
      participants: friends.filter((f) =>
        currentSession.participants.includes(f.id)
      ),
      timestamp: new Date(),
    };

    setMatchHistory([history, ...matchHistory]);
    setCurrentSession(completedSession);
  };

  const getContentForCategory = (category: Category): ContentItem[] => {
    switch (category) {
      case 'movies':
        return mockMovies;
      case 'restaurants':
        return mockRestaurants;
      case 'recipes':
        return mockRecipes;
      case 'boardgames':
        return mockBoardGames;
      default:
        return [];
    }
  };

  return (
    <AppContext.Provider
      value={{
        currentUser,
        friends,
        friendRequests,
        matchHistory,
        currentSession,
        isAuthenticated,
        login,
        logout,
        register,
        acceptFriendRequest,
        rejectFriendRequest,
        addFriend,
        createSession,
        joinSession,
        recordSwipe,
        completeSession,
        getContentForCategory,
        currentSwipeIndex,
        setCurrentSwipeIndex,
        sessionSwipes,
      }}
    >
      {children}
    </AppContext.Provider>
  );
};

export const useApp = () => {
  const context = useContext(AppContext);
  if (!context) {
    throw new Error('useApp must be used within AppProvider');
  }
  return context;
};
