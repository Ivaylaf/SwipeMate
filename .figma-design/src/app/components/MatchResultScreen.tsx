import React from 'react';
import { Card } from './ui/card';
import { Button } from './ui/button';
import { Avatar, AvatarFallback, AvatarImage } from './ui/avatar';
import { Badge } from './ui/badge';
import { PartyPopper, Home, Star } from 'lucide-react';
import { useApp } from '../context/AppContext';
import { Movie, Restaurant, Recipe, BoardGame } from '../types';
import { motion } from 'motion/react';

interface MatchResultScreenProps {
  onBackToHome: () => void;
}

export const MatchResultScreen: React.FC<MatchResultScreenProps> = ({ onBackToHome }) => {
  const { currentSession, friends } = useApp();

  if (!currentSession || !currentSession.result) return null;

  const result = currentSession.result;
  const sessionParticipants = friends.filter((f) =>
    currentSession.participants.includes(f.id)
  );

  const renderDetails = () => {
    if ('title' in result) {
      const movie = result as Movie;
      return (
        <div className="space-y-2">
          <div className="flex items-center space-x-2">
            <Star className="h-4 w-4 fill-yellow-400 text-yellow-400" />
            <span className="font-semibold">{movie.rating.toFixed(1)}</span>
            <span className="text-gray-500">• {movie.year} • {movie.duration} min</span>
          </div>
          <div className="flex flex-wrap gap-2">
            {movie.genre.map((g) => (
              <Badge key={g} variant="secondary">{g}</Badge>
            ))}
          </div>
        </div>
      );
    }
    
    if ('cuisine' in result && 'city' in result) {
      const restaurant = result as Restaurant;
      return (
        <div className="space-y-2">
          <div className="flex items-center space-x-2">
            <Star className="h-4 w-4 fill-yellow-400 text-yellow-400" />
            <span className="font-semibold">{restaurant.rating.toFixed(1)}</span>
            <span className="text-gray-500">• {restaurant.priceRange}</span>
          </div>
          <div className="flex flex-wrap gap-2">
            {restaurant.cuisine.map((c) => (
              <Badge key={c} variant="secondary">{c}</Badge>
            ))}
          </div>
          <p className="text-gray-600">{restaurant.district}, {restaurant.city}</p>
        </div>
      );
    }
    
    if ('difficulty' in result) {
      const recipe = result as Recipe;
      return (
        <div className="space-y-2">
          <div className="flex items-center space-x-2">
            <Star className="h-4 w-4 fill-yellow-400 text-yellow-400" />
            <span className="font-semibold">{recipe.rating.toFixed(1)}</span>
            <span className="text-gray-500">• {recipe.prepTime} min • {recipe.difficulty}</span>
          </div>
          <div className="flex flex-wrap gap-2">
            {recipe.cuisine.map((c) => (
              <Badge key={c} variant="secondary">{c}</Badge>
            ))}
          </div>
        </div>
      );
    }
    
    if ('players' in result) {
      const game = result as BoardGame;
      return (
        <div className="space-y-2">
          <div className="flex items-center space-x-2">
            <Star className="h-4 w-4 fill-yellow-400 text-yellow-400" />
            <span className="font-semibold">{game.rating.toFixed(1)}</span>
            <span className="text-gray-500">• {game.duration} min • {game.players.min}-{game.players.max} players</span>
          </div>
          <div className="flex flex-wrap gap-2">
            {game.gameType.map((t) => (
              <Badge key={t} variant="secondary">{t}</Badge>
            ))}
          </div>
        </div>
      );
    }

    return null;
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-pink-50 to-purple-50 flex items-center justify-center p-4">
      <motion.div
        initial={{ scale: 0.8, opacity: 0 }}
        animate={{ scale: 1, opacity: 1 }}
        transition={{ duration: 0.5 }}
        className="w-full max-w-2xl"
      >
        <Card className="overflow-hidden">
          {/* Celebration Header */}
          <div className="bg-gradient-to-r from-pink-500 to-purple-500 p-8 text-center text-white">
            <motion.div
              animate={{ rotate: [0, 10, -10, 10, 0] }}
              transition={{ duration: 0.5, delay: 0.3 }}
            >
              <PartyPopper className="h-16 w-16 mx-auto mb-4" />
            </motion.div>
            <h1 className="text-3xl font-bold mb-2">It's a Match!</h1>
            <p className="text-white/90">You all agreed on this choice</p>
          </div>

          {/* Result Image */}
          <div className="relative h-64">
            <img
              src={result.image}
              alt={result.name || (result as Movie).title}
              className="w-full h-full object-cover"
            />
            <div className="absolute inset-0 bg-gradient-to-t from-black/60 to-transparent" />
          </div>

          {/* Result Details */}
          <div className="p-8 space-y-6">
            <div>
              <h2 className="text-3xl font-bold mb-4">
                {result.name || (result as Movie).title}
              </h2>
              {renderDetails()}
            </div>

            {'description' in result && (
              <p className="text-gray-700">{result.description}</p>
            )}

            {/* Participants */}
            {sessionParticipants.length > 0 && (
              <div>
                <p className="text-sm text-gray-600 mb-3">Matched with:</p>
                <div className="flex flex-wrap gap-3">
                  {sessionParticipants.map((participant) => (
                    <div key={participant.id} className="flex items-center space-x-2">
                      <Avatar className="h-8 w-8">
                        <AvatarImage src={participant.profilePicture} />
                        <AvatarFallback>
                          {participant.username.substring(0, 2).toUpperCase()}
                        </AvatarFallback>
                      </Avatar>
                      <span className="text-sm font-medium">{participant.username}</span>
                    </div>
                  ))}
                </div>
              </div>
            )}

            {/* Actions */}
            <div className="flex space-x-4">
              <Button
                onClick={onBackToHome}
                className="flex-1 bg-gradient-to-r from-pink-500 to-purple-500"
              >
                <Home className="mr-2 h-5 w-5" />
                Back to Home
              </Button>
            </div>
          </div>
        </Card>
      </motion.div>
    </div>
  );
};