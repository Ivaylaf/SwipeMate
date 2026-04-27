import React, { useState } from 'react';
import { Card } from './ui/card';
import { Button } from './ui/button';
import { Avatar, AvatarFallback, AvatarImage } from './ui/avatar';
import { Checkbox } from './ui/checkbox';
import { ArrowLeft, ArrowRight, Film, UtensilsCrossed, ChefHat, Dices } from 'lucide-react';
import { useApp } from '../context/AppContext';
import { Category } from '../types';

interface CreateSessionScreenProps {
  category: Category;
  onBack: () => void;
  onContinue: () => void;
}

export const CreateSessionScreen: React.FC<CreateSessionScreenProps> = ({
  category,
  onBack,
  onContinue,
}) => {
  const { friends, createSession } = useApp();
  const [selectedFriends, setSelectedFriends] = useState<string[]>([]);

  const categoryInfo = {
    movies: { name: 'Movies & TV', icon: Film, color: 'from-blue-500 to-cyan-500' },
    restaurants: { name: 'Restaurants', icon: UtensilsCrossed, color: 'from-orange-500 to-red-500' },
    recipes: { name: 'Recipes', icon: ChefHat, color: 'from-green-500 to-emerald-500' },
    boardgames: { name: 'Board Games', icon: Dices, color: 'from-purple-500 to-pink-500' },
  };

  const info = categoryInfo[category];
  const Icon = info.icon;

  const toggleFriend = (friendId: string) => {
    setSelectedFriends((prev) =>
      prev.includes(friendId)
        ? prev.filter((id) => id !== friendId)
        : [...prev, friendId]
    );
  };

  const handleContinue = () => {
    createSession(category, selectedFriends, {});
    onContinue();
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-pink-50 to-purple-50">
      {/* Header */}
      <div className="bg-white border-b">
        <div className="max-w-4xl mx-auto px-4 py-4 flex items-center space-x-4">
          <Button variant="ghost" size="icon" onClick={onBack}>
            <ArrowLeft className="h-5 w-5" />
          </Button>
          <div className="flex-1">
            <h1 className="text-2xl font-bold">Create Match Session</h1>
          </div>
        </div>
      </div>

      <div className="max-w-4xl mx-auto px-4 py-8 space-y-6">
        {/* Category Info */}
        <Card className="p-6">
          <div className="flex items-center space-x-4">
            <div className={`bg-gradient-to-br ${info.color} p-4 rounded-full`}>
              <Icon className="h-8 w-8 text-white" />
            </div>
            <div>
              <h2 className="text-xl font-bold">{info.name}</h2>
              <p className="text-gray-600">Select friends to match with</p>
            </div>
          </div>
        </Card>

        {/* Select Friends */}
        <Card className="p-6">
          <h3 className="font-semibold mb-4">
            Select Friends ({selectedFriends.length} selected)
          </h3>
          
          {friends.length === 0 ? (
            <div className="text-center py-8 text-gray-500">
              <p>You don't have any friends yet.</p>
              <p className="text-sm">Add friends to start matching!</p>
            </div>
          ) : (
            <div className="space-y-3">
              {friends.map((friend) => (
                <div
                  key={friend.id}
                  className="flex items-center space-x-4 p-3 rounded-lg hover:bg-gray-50 cursor-pointer"
                  onClick={() => toggleFriend(friend.id)}
                >
                  <Checkbox
                    checked={selectedFriends.includes(friend.id)}
                    onCheckedChange={() => toggleFriend(friend.id)}
                  />
                  <Avatar className="h-10 w-10">
                    <AvatarImage src={friend.profilePicture} />
                    <AvatarFallback>
                      {friend.username.substring(0, 2).toUpperCase()}
                    </AvatarFallback>
                  </Avatar>
                  <div className="flex-1">
                    <p className="font-medium">{friend.username}</p>
                    <p className="text-sm text-gray-500">
                      {friend.status === 'active' ? '🟢 Online' : '⚫ Offline'}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          )}
        </Card>

        {/* Start Session Button */}
        <div className="flex space-x-4">
          <Button
            variant="outline"
            onClick={onBack}
            className="flex-1"
          >
            Cancel
          </Button>
          <Button
            onClick={handleContinue}
            className={`flex-1 bg-gradient-to-r ${info.color}`}
            disabled={selectedFriends.length === 0}
          >
            Continue to Filters
            <ArrowRight className="ml-2 h-5 w-5" />
          </Button>
        </div>

        {selectedFriends.length === 0 && (
          <p className="text-center text-sm text-gray-500">
            Note: You can also start a solo session without selecting friends
          </p>
        )}
      </div>
    </div>
  );
};
