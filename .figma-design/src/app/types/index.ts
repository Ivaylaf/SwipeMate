// Type definitions for SwipeMate

export type Category = 'movies' | 'restaurants' | 'recipes' | 'boardgames';

export interface User {
  id: string;
  username: string;
  email: string;
  profilePicture?: string;
  bio?: string;
  reviewCount: number;
  ratingCount: number;
  badges: Badge[];
}

export type Badge = 
  | { type: 'reviewer1'; name: 'Reviewer Level 1'; description: '10+ reviews' }
  | { type: 'active'; name: 'Active User'; description: '50+ ratings' };

export interface Friend {
  id: string;
  username: string;
  profilePicture?: string;
  status: 'active' | 'offline';
}

export interface FriendRequest {
  id: string;
  fromUser: Friend;
  timestamp: Date;
}

export interface Movie {
  id: string;
  title: string;
  image: string;
  rating: number;
  genre: string[];
  year: number;
  duration: number; // in minutes
  description: string;
  type: 'movie' | 'series';
}

export interface Restaurant {
  id: string;
  name: string;
  image: string;
  rating: number;
  cuisine: string[];
  city: string;
  district: string;
  description: string;
  priceRange: string;
}

export interface Recipe {
  id: string;
  name: string;
  image: string;
  rating: number;
  difficulty: 'easy' | 'medium' | 'hard';
  cuisine: string[];
  mealType: string[];
  budget: 'low' | 'medium' | 'high';
  ingredients: string[];
  description: string;
  prepTime: number; // in minutes
}

export interface BoardGame {
  id: string;
  name: string;
  image: string;
  rating: number;
  gameType: string[];
  duration: number; // in minutes
  players: { min: number; max: number };
  description: string;
  complexity: number; // 1-5
}

export type ContentItem = Movie | Restaurant | Recipe | BoardGame;

export interface MovieFilters {
  rating: number[];
  genre: string[];
  year: number[];
  duration: number[];
}

export interface RestaurantFilters {
  rating: number[];
  cuisine: string[];
  city: string;
  district: string[];
}

export interface RecipeFilters {
  rating: number[];
  difficulty: string[];
  cuisine: string[];
  mealType: string[];
  budget: string[];
}

export interface BoardGameFilters {
  rating: number[];
  gameType: string[];
  duration: number[];
  players: number[];
}

export type Filters = MovieFilters | RestaurantFilters | RecipeFilters | BoardGameFilters;

export interface MatchSession {
  id: string;
  category: Category;
  createdBy: string;
  participants: string[];
  filters: Filters;
  status: 'waiting' | 'active' | 'completed';
  result?: ContentItem;
  createdAt: Date;
}

export interface SwipeAction {
  userId: string;
  itemId: string;
  action: 'like' | 'reject';
}

export interface MatchHistory {
  id: string;
  session: MatchSession;
  result: ContentItem;
  participants: Friend[];
  timestamp: Date;
}

export interface Review {
  id: string;
  userId: string;
  username: string;
  itemId: string;
  category: Category;
  rating: number;
  text: string;
  timestamp: Date;
}
