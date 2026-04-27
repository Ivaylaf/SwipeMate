import React from 'react';
import { Card } from './ui/card';
import { Button } from './ui/button';
import { Avatar, AvatarFallback, AvatarImage } from './ui/avatar';
import { Badge } from './ui/badge';
import { ArrowLeft, Film, UtensilsCrossed, ChefHat, Dices, Star } from 'lucide-react';
import { useApp } from '../context/AppContext';
import { Movie, Restaurant, Recipe, BoardGame } from '../types';

interface HistoryScreenProps {
  onBack: () => void;
}

export const HistoryScreen: React.FC<HistoryScreenProps> = ({ onBack }) => {
  const { matchHistory } = useApp();

  const getCategoryIcon = (category: string) => {
    switch (category) {
      case 'movies':
        return Film;
      case 'restaurants':
        return UtensilsCrossed;
      case 'recipes':
        return ChefHat;
      case 'boardgames':
        return Dices;
      default:
        return Film;
    }
  };

  const getCategoryColor = (category: string) => {
    switch (category) {
      case 'movies':
        return 'from-blue-500 to-cyan-500';
      case 'restaurants':
        return 'from-orange-500 to-red-500';
      case 'recipes':
        return 'from-green-500 to-emerald-500';
      case 'boardgames':
        return 'from-purple-500 to-pink-500';
      default:
        return 'from-gray-500 to-gray-600';
    }
  };

  const formatDate = (date: Date) => {
    const now = new Date();
    const diff = now.getTime() - new Date(date).getTime();
    const days = Math.floor(diff / (1000 * 60 * 60 * 24));
    
    if (days === 0) return 'Today';
    if (days === 1) return 'Yesterday';
    if (days < 7) return `${days} days ago`;
    return new Date(date).toLocaleDateString();
  };

  const getRating = (result: any) => {
    return result.rating ? result.rating.toFixed(1) : 'N/A';
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-pink-50 to-purple-50">
      {/* Header */}
      <div className="bg-white border-b">
        <div className="max-w-4xl mx-auto px-4 py-4 flex items-center space-x-4">
          <Button variant="ghost" size="icon" onClick={onBack}>
            <ArrowLeft className="h-5 w-5" />
          </Button>
          <h1 className="text-2xl font-bold">Match History</h1>
        </div>
      </div>

      <div className="max-w-4xl mx-auto px-4 py-8">
        {matchHistory.length === 0 ? (
          <Card className="p-12 text-center">
            <p className="text-gray-500 mb-4">No match history yet</p>
            <p className="text-sm text-gray-400">
              Start creating match sessions to see your history here!
            </p>
          </Card>
        ) : (
          <div className="space-y-4">
            {matchHistory.map((history) => {
              const Icon = getCategoryIcon(history.session.category);
              const colorClass = getCategoryColor(history.session.category);
              const result = history.result;

              return (
                <Card key={history.id} className="overflow-hidden hover:shadow-lg transition-shadow">
                  <div className="flex">
                    {/* Image Section */}
                    <div className="relative w-32 h-32 flex-shrink-0">
                      <img
                        src={result.image}
                        alt={result.name || (result as Movie).title}
                        className="w-full h-full object-cover"
                      />
                      <div className={`absolute top-2 left-2 bg-gradient-to-br ${colorClass} p-2 rounded-full`}>
                        <Icon className="h-4 w-4 text-white" />
                      </div>
                    </div>

                    {/* Content Section */}
                    <div className="flex-1 p-4 space-y-2">
                      <div className="flex items-start justify-between">
                        <div className="flex-1">
                          <h3 className="font-bold text-lg">
                            {result.name || (result as Movie).title}
                          </h3>
                          <div className="flex items-center space-x-2 text-sm text-gray-600">
                            <Star className="h-4 w-4 fill-yellow-400 text-yellow-400" />
                            <span>{getRating(result)}</span>
                          </div>
                        </div>
                        <p className="text-xs text-gray-500">
                          {formatDate(history.timestamp)}
                        </p>
                      </div>

                      {/* Participants */}
                      {history.participants.length > 0 && (
                        <div className="flex items-center space-x-1">
                          <p className="text-xs text-gray-500 mr-2">With:</p>
                          <div className="flex -space-x-2">
                            {history.participants.slice(0, 3).map((participant) => (
                              <Avatar key={participant.id} className="h-6 w-6 border-2 border-white">
                                <AvatarImage src={participant.profilePicture} />
                                <AvatarFallback className="text-xs">
                                  {participant.username.substring(0, 2).toUpperCase()}
                                </AvatarFallback>
                              </Avatar>
                            ))}
                          </div>
                          {history.participants.length > 3 && (
                            <span className="text-xs text-gray-500 ml-1">
                              +{history.participants.length - 3} more
                            </span>
                          )}
                        </div>
                      )}

                      {/* Tags */}
                      <div className="flex flex-wrap gap-1">
                        {(() => {
                          if ('genre' in result) {
                            const movie = result as Movie;
                            return movie.genre.slice(0, 2).map((g) => (
                              <Badge key={g} variant="secondary" className="text-xs">
                                {g}
                              </Badge>
                            ));
                          }
                          if ('cuisine' in result && 'city' in result) {
                            const restaurant = result as Restaurant;
                            return restaurant.cuisine.slice(0, 2).map((c) => (
                              <Badge key={c} variant="secondary" className="text-xs">
                                {c}
                              </Badge>
                            ));
                          }
                          if ('difficulty' in result) {
                            const recipe = result as Recipe;
                            return [
                              <Badge key="diff" variant="secondary" className="text-xs capitalize">
                                {recipe.difficulty}
                              </Badge>,
                            ];
                          }
                          if ('players' in result) {
                            const game = result as BoardGame;
                            return game.gameType.slice(0, 2).map((t) => (
                              <Badge key={t} variant="secondary" className="text-xs">
                                {t}
                              </Badge>
                            ));
                          }
                          return null;
                        })()}
                      </div>
                    </div>
                  </div>
                </Card>
              );
            })}
          </div>
        )}
      </div>
    </div>
  );
};
