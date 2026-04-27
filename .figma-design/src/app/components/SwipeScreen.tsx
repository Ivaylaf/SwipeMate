import React, { useState, useEffect } from 'react';
import { Card } from './ui/card';
import { Button } from './ui/button';
import { Badge } from './ui/badge';
import { ArrowLeft, ThumbsUp, X, Star, Clock, Users, MapPin } from 'lucide-react';
import { useApp } from '../context/AppContext';
import { ContentItem, Movie, Restaurant, Recipe, BoardGame } from '../types';
import { motion, AnimatePresence, useMotionValue, useTransform } from 'motion/react';

interface SwipeScreenProps {
  onBack: () => void;
}

export const SwipeScreen: React.FC<SwipeScreenProps> = ({ onBack }) => {
  const { currentSession, getContentForCategory, recordSwipe, currentSwipeIndex, setCurrentSwipeIndex } = useApp();
  const [items, setItems] = useState<ContentItem[]>([]);
  
  const x = useMotionValue(0);
  const rotate = useTransform(x, [-200, 200], [-25, 25]);
  const opacity = useTransform(x, [-200, -100, 0, 100, 200], [0, 1, 1, 1, 0]);

  useEffect(() => {
    if (currentSession) {
      const content = getContentForCategory(currentSession.category);
      setItems(content);
    }
  }, [currentSession]);

  if (!currentSession || items.length === 0) return null;

  const currentItem = items[currentSwipeIndex];

  if (!currentItem) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-pink-50 to-purple-50 flex items-center justify-center">
        <Card className="p-12 text-center">
          <h2 className="text-2xl font-bold mb-2">No more items</h2>
          <p className="text-gray-600 mb-6">You've gone through all available options!</p>
          <Button onClick={onBack}>Back to Home</Button>
        </Card>
      </div>
    );
  }

  const handleSwipe = (direction: 'like' | 'reject') => {
    recordSwipe(currentItem.id, direction);
    setCurrentSwipeIndex(currentSwipeIndex + 1);
    x.set(0);
  };

  const handleDragEnd = (event: any, info: any) => {
    if (info.offset.x > 100) {
      handleSwipe('like');
    } else if (info.offset.x < -100) {
      handleSwipe('reject');
    }
  };

  const renderItemDetails = () => {
    const category = currentSession.category;

    if (category === 'movies') {
      const movie = currentItem as Movie;
      return (
        <div className="space-y-3">
          <div className="flex items-center space-x-2">
            <Badge>{movie.type === 'movie' ? 'Movie' : 'TV Series'}</Badge>
            <Badge variant="outline">{movie.year}</Badge>
          </div>
          <div className="flex flex-wrap gap-2">
            {movie.genre.map((g) => (
              <Badge key={g} variant="secondary">{g}</Badge>
            ))}
          </div>
          <div className="flex items-center space-x-4 text-sm text-gray-600">
            <div className="flex items-center">
              <Star className="h-4 w-4 mr-1 fill-yellow-400 text-yellow-400" />
              {movie.rating.toFixed(1)}
            </div>
            <div className="flex items-center">
              <Clock className="h-4 w-4 mr-1" />
              {movie.duration} min
            </div>
          </div>
          <p className="text-gray-700">{movie.description}</p>
        </div>
      );
    }

    if (category === 'restaurants') {
      const restaurant = currentItem as Restaurant;
      return (
        <div className="space-y-3">
          <div className="flex items-center space-x-2">
            <Badge>{restaurant.priceRange}</Badge>
          </div>
          <div className="flex flex-wrap gap-2">
            {restaurant.cuisine.map((c) => (
              <Badge key={c} variant="secondary">{c}</Badge>
            ))}
          </div>
          <div className="flex items-center space-x-4 text-sm text-gray-600">
            <div className="flex items-center">
              <Star className="h-4 w-4 mr-1 fill-yellow-400 text-yellow-400" />
              {restaurant.rating.toFixed(1)}
            </div>
            <div className="flex items-center">
              <MapPin className="h-4 w-4 mr-1" />
              {restaurant.district}, {restaurant.city}
            </div>
          </div>
          <p className="text-gray-700">{restaurant.description}</p>
        </div>
      );
    }

    if (category === 'recipes') {
      const recipe = currentItem as Recipe;
      return (
        <div className="space-y-3">
          <div className="flex items-center space-x-2">
            <Badge className="capitalize">{recipe.difficulty}</Badge>
            <Badge variant="outline" className="capitalize">{recipe.budget} budget</Badge>
          </div>
          <div className="flex flex-wrap gap-2">
            {recipe.cuisine.map((c) => (
              <Badge key={c} variant="secondary">{c}</Badge>
            ))}
            {recipe.mealType.map((m) => (
              <Badge key={m} variant="secondary">{m}</Badge>
            ))}
          </div>
          <div className="flex items-center space-x-4 text-sm text-gray-600">
            <div className="flex items-center">
              <Star className="h-4 w-4 mr-1 fill-yellow-400 text-yellow-400" />
              {recipe.rating.toFixed(1)}
            </div>
            <div className="flex items-center">
              <Clock className="h-4 w-4 mr-1" />
              {recipe.prepTime} min
            </div>
          </div>
          <p className="text-gray-700">{recipe.description}</p>
        </div>
      );
    }

    if (category === 'boardgames') {
      const game = currentItem as BoardGame;
      return (
        <div className="space-y-3">
          <div className="flex items-center space-x-2">
            <Badge>Complexity: {game.complexity}/5</Badge>
          </div>
          <div className="flex flex-wrap gap-2">
            {game.gameType.map((t) => (
              <Badge key={t} variant="secondary">{t}</Badge>
            ))}
          </div>
          <div className="flex items-center space-x-4 text-sm text-gray-600">
            <div className="flex items-center">
              <Star className="h-4 w-4 mr-1 fill-yellow-400 text-yellow-400" />
              {game.rating.toFixed(1)}
            </div>
            <div className="flex items-center">
              <Clock className="h-4 w-4 mr-1" />
              {game.duration} min
            </div>
            <div className="flex items-center">
              <Users className="h-4 w-4 mr-1" />
              {game.players.min}-{game.players.max}
            </div>
          </div>
          <p className="text-gray-700">{game.description}</p>
        </div>
      );
    }

    return null;
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-pink-50 to-purple-50">
      {/* Header */}
      <div className="bg-white border-b">
        <div className="max-w-4xl mx-auto px-4 py-4 flex items-center space-x-4">
          <Button variant="ghost" size="icon" onClick={onBack}>
            <ArrowLeft className="h-5 w-5" />
          </Button>
          <div className="flex-1 text-center">
            <p className="text-sm text-gray-600">
              {currentSwipeIndex + 1} / {items.length}
            </p>
          </div>
        </div>
      </div>

      <div className="max-w-4xl mx-auto px-4 py-8">
        {/* Swipe Card */}
        <div className="relative h-[600px] mb-8">
          <AnimatePresence>
            <motion.div
              key={currentItem.id}
              className="absolute inset-0"
              style={{ x, rotate, opacity }}
              drag="x"
              dragConstraints={{ left: 0, right: 0 }}
              onDragEnd={handleDragEnd}
              whileTap={{ cursor: 'grabbing' }}
            >
              <Card className="h-full overflow-hidden cursor-grab active:cursor-grabbing">
                <div className="relative h-1/2">
                  <img
                    src={currentItem.image}
                    alt={currentItem.name || (currentItem as Movie).title}
                    className="w-full h-full object-cover"
                  />
                  <div className="absolute inset-0 bg-gradient-to-t from-black/60 to-transparent" />
                  <div className="absolute bottom-4 left-4 right-4">
                    <h2 className="text-3xl font-bold text-white">
                      {currentItem.name || (currentItem as Movie).title}
                    </h2>
                  </div>
                </div>
                <div className="p-6 h-1/2 overflow-y-auto">
                  {renderItemDetails()}
                </div>
              </Card>
            </motion.div>
          </AnimatePresence>
        </div>

        {/* Action Buttons */}
        <div className="flex justify-center space-x-8">
          <Button
            size="lg"
            variant="outline"
            onClick={() => handleSwipe('reject')}
            className="h-16 w-16 rounded-full border-2 border-red-500 hover:bg-red-50"
          >
            <X className="h-8 w-8 text-red-500" />
          </Button>
          <Button
            size="lg"
            onClick={() => handleSwipe('like')}
            className="h-16 w-16 rounded-full bg-gradient-to-r from-pink-500 to-purple-500"
          >
            <ThumbsUp className="h-8 w-8 text-white" />
          </Button>
        </div>

        {/* Swipe Hint */}
        <p className="text-center text-gray-500 mt-6 text-sm">
          Swipe right or tap 👍 to like • Swipe left or tap ✕ to skip
        </p>
      </div>
    </div>
  );
};