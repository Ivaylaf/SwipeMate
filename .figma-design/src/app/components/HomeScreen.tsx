import React from 'react';
import { Card } from './ui/card';
import { Button } from './ui/button';
import { Film, UtensilsCrossed, ChefHat, Dices, User, History, LogOut, HelpCircle } from 'lucide-react';
import { useApp } from '../context/AppContext';
import { Category } from '../types';

interface HomeScreenProps {
  onSelectCategory: (category: Category) => void;
  onNavigateToFriends: () => void;
  onNavigateToHistory: () => void;
  onNavigateToProfile: () => void;
  onShowHelp?: () => void;
}

export const HomeScreen: React.FC<HomeScreenProps> = ({
  onSelectCategory,
  onNavigateToFriends,
  onNavigateToHistory,
  onNavigateToProfile,
  onShowHelp,
}) => {
  const { currentUser, logout, friendRequests } = useApp();

  const categories = [
    {
      id: 'movies' as Category,
      name: 'Movies & TV',
      icon: Film,
      color: 'from-blue-500 to-cyan-500',
      image: 'https://images.unsplash.com/photo-1517604931442-7e0c8ed2963c?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxtb3ZpZSUyMHRoZWF0ZXJ8ZW58MXx8fHwxNjc3MTYzMjIyfDA&ixlib=rb-4.1.0&q=80&w=400',
    },
    {
      id: 'restaurants' as Category,
      name: 'Restaurants',
      icon: UtensilsCrossed,
      color: 'from-orange-500 to-red-500',
      image: 'https://images.unsplash.com/photo-1414235077428-338989a2e8c0?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxyZXN0YXVyYW50JTIwZGluaW5nfGVufDF8fHx8MTc2NzEzNTIzNHww&ixlib=rb-4.1.0&q=80&w=400',
    },
    {
      id: 'recipes' as Category,
      name: 'Recipes',
      icon: ChefHat,
      color: 'from-green-500 to-emerald-500',
      image: 'https://images.unsplash.com/photo-1514986888952-8cd320577b68?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxjb29raW5nJTIwZm9vZHxlbnwxfHx8fDE3NjcxNjIwODd8MA&ixlib=rb-4.1.0&q=80&w=400',
    },
    {
      id: 'boardgames' as Category,
      name: 'Board Games',
      icon: Dices,
      color: 'from-purple-500 to-pink-500',
      image: 'https://images.unsplash.com/photo-1629760946220-5693ee4c46ac?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxib2FyZCUyMGdhbWVzfGVufDF8fHx8MTc2NzE3MDkwNnww&ixlib=rb-4.1.0&q=80&w=400',
    },
  ];

  return (
    <div className="min-h-screen bg-gradient-to-br from-pink-50 to-purple-50">
      {/* Header */}
      <div className="bg-white border-b">
        <div className="max-w-7xl mx-auto px-4 py-4 flex items-center justify-between">
          <div className="flex items-center space-x-2">
            <div className="bg-gradient-to-br from-pink-500 to-purple-500 p-2 rounded-full">
              <Film className="h-6 w-6 text-white" />
            </div>
            <h1 className="text-2xl font-bold bg-gradient-to-r from-pink-500 to-purple-500 bg-clip-text text-transparent">
              SwipeMate
            </h1>
          </div>
          <div className="flex items-center space-x-2">
            {onShowHelp && (
              <Button variant="ghost" size="icon" onClick={onShowHelp}>
                <HelpCircle className="h-5 w-5" />
              </Button>
            )}
            <Button variant="ghost" size="icon" onClick={logout}>
              <LogOut className="h-5 w-5" />
            </Button>
          </div>
        </div>
      </div>

      <div className="max-w-7xl mx-auto px-4 py-8 space-y-8">
        {/* Welcome Section */}
        <div className="text-center space-y-2">
          <h2 className="text-3xl font-bold">
            Welcome back, {currentUser?.username}!
          </h2>
          <p className="text-gray-600">
            Choose a category to start matching with friends
          </p>
        </div>

        {/* Category Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {categories.map((category) => {
            const Icon = category.icon;
            return (
              <Card
                key={category.id}
                className="overflow-hidden cursor-pointer hover:shadow-lg transition-shadow group"
                onClick={() => onSelectCategory(category.id)}
              >
                <div className="relative h-48">
                  <img
                    src={category.image}
                    alt={category.name}
                    className="w-full h-full object-cover"
                  />
                  <div className={`absolute inset-0 bg-gradient-to-br ${category.color} opacity-60 group-hover:opacity-70 transition-opacity`} />
                  <div className="absolute inset-0 flex items-center justify-center">
                    <div className="text-center text-white">
                      <Icon className="h-16 w-16 mx-auto mb-2" />
                      <h3 className="text-2xl font-bold">{category.name}</h3>
                    </div>
                  </div>
                </div>
              </Card>
            );
          })}
        </div>

        {/* Quick Actions */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <Button
            variant="outline"
            className="w-full h-20 relative"
            onClick={onNavigateToFriends}
          >
            <User className="mr-2 h-5 w-5" />
            Friends
            {friendRequests.length > 0 && (
              <span className="absolute top-2 right-2 bg-red-500 text-white text-xs rounded-full h-6 w-6 flex items-center justify-center">
                {friendRequests.length}
              </span>
            )}
          </Button>
          <Button
            variant="outline"
            className="w-full h-20"
            onClick={onNavigateToHistory}
          >
            <History className="mr-2 h-5 w-5" />
            Match History
          </Button>
          <Button
            variant="outline"
            className="w-full h-20"
            onClick={onNavigateToProfile}
          >
            <User className="mr-2 h-5 w-5" />
            My Profile
          </Button>
        </div>
      </div>
    </div>
  );
};