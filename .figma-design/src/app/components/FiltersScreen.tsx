import React, { useState } from 'react';
import { Card } from './ui/card';
import { Button } from './ui/button';
import { Badge } from './ui/badge';
import { Slider } from './ui/slider';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from './ui/select';
import { ArrowLeft, Play } from 'lucide-react';
import { useApp } from '../context/AppContext';
import { Category } from '../types';

interface FiltersScreenProps {
  onBack: () => void;
  onStartSwipe: () => void;
}

export const FiltersScreen: React.FC<FiltersScreenProps> = ({ onBack, onStartSwipe }) => {
  const { currentSession } = useApp();
  
  if (!currentSession) return null;

  const category = currentSession.category;

  // Movie/TV filters
  const [movieGenres, setMovieGenres] = useState<string[]>([]);
  const [minRating, setMinRating] = useState(0);
  const [yearRange, setYearRange] = useState<[number, number]>([1990, 2024]);

  // Restaurant filters
  const [restaurantCuisines, setRestaurantCuisines] = useState<string[]>([]);
  const [city, setCity] = useState('Sofia');
  const [district, setDistrict] = useState('all');

  // Recipe filters
  const [recipeCuisines, setRecipeCuisines] = useState<string[]>([]);
  const [difficulty, setDifficulty] = useState('all');
  const [mealType, setMealType] = useState('all');
  const [budget, setBudget] = useState('all');

  // Board game filters
  const [gameTypes, setGameTypes] = useState<string[]>([]);
  const [players, setPlayers] = useState<[number, number]>([2, 8]);

  const toggleGenre = (genre: string, current: string[], setter: (val: string[]) => void) => {
    setter(
      current.includes(genre)
        ? current.filter((g) => g !== genre)
        : [...current, genre]
    );
  };

  const renderMovieFilters = () => {
    const genres = ['Action', 'Comedy', 'Drama', 'Sci-Fi', 'Horror', 'Romance', 'Thriller'];
    return (
      <div className="space-y-6">
        <div>
          <label className="font-medium mb-3 block">Genres</label>
          <div className="flex flex-wrap gap-2">
            {genres.map((genre) => (
              <Badge
                key={genre}
                variant={movieGenres.includes(genre) ? 'default' : 'outline'}
                className="cursor-pointer"
                onClick={() => toggleGenre(genre, movieGenres, setMovieGenres)}
              >
                {genre}
              </Badge>
            ))}
          </div>
        </div>

        <div>
          <label className="font-medium mb-3 block">
            Minimum Rating: {minRating.toFixed(1)}+
          </label>
          <Slider
            value={[minRating]}
            onValueChange={(val) => setMinRating(val[0])}
            min={0}
            max={10}
            step={0.5}
          />
        </div>

        <div>
          <label className="font-medium mb-3 block">
            Release Year: {yearRange[0]} - {yearRange[1]}
          </label>
          <Slider
            value={yearRange}
            onValueChange={(val) => setYearRange(val as [number, number])}
            min={1970}
            max={2024}
            step={1}
          />
        </div>
      </div>
    );
  };

  const renderRestaurantFilters = () => {
    const cuisines = ['Bulgarian', 'Italian', 'Asian', 'Mediterranean', 'Vegetarian', 'Steakhouse'];
    const districts = ['Center', 'Lozenets', 'Vitosha', 'Oborishte', 'Mladost'];
    
    return (
      <div className="space-y-6">
        <div>
          <label className="font-medium mb-3 block">Cuisine Types</label>
          <div className="flex flex-wrap gap-2">
            {cuisines.map((cuisine) => (
              <Badge
                key={cuisine}
                variant={restaurantCuisines.includes(cuisine) ? 'default' : 'outline'}
                className="cursor-pointer"
                onClick={() => toggleGenre(cuisine, restaurantCuisines, setRestaurantCuisines)}
              >
                {cuisine}
              </Badge>
            ))}
          </div>
        </div>

        <div>
          <label className="font-medium mb-3 block">District (Sofia)</label>
          <Select value={district} onValueChange={setDistrict}>
            <SelectTrigger>
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Districts</SelectItem>
              {districts.map((d) => (
                <SelectItem key={d} value={d}>{d}</SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <div>
          <label className="font-medium mb-3 block">
            Minimum Rating: {minRating.toFixed(1)}+
          </label>
          <Slider
            value={[minRating]}
            onValueChange={(val) => setMinRating(val[0])}
            min={0}
            max={5}
            step={0.5}
          />
        </div>
      </div>
    );
  };

  const renderRecipeFilters = () => {
    const cuisines = ['Bulgarian', 'Italian', 'Asian', 'Greek', 'Vegetarian', 'Healthy'];
    
    return (
      <div className="space-y-6">
        <div>
          <label className="font-medium mb-3 block">Cuisine Types</label>
          <div className="flex flex-wrap gap-2">
            {cuisines.map((cuisine) => (
              <Badge
                key={cuisine}
                variant={recipeCuisines.includes(cuisine) ? 'default' : 'outline'}
                className="cursor-pointer"
                onClick={() => toggleGenre(cuisine, recipeCuisines, setRecipeCuisines)}
              >
                {cuisine}
              </Badge>
            ))}
          </div>
        </div>

        <div>
          <label className="font-medium mb-3 block">Difficulty</label>
          <Select value={difficulty} onValueChange={setDifficulty}>
            <SelectTrigger>
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Levels</SelectItem>
              <SelectItem value="easy">Easy</SelectItem>
              <SelectItem value="medium">Medium</SelectItem>
              <SelectItem value="hard">Hard</SelectItem>
            </SelectContent>
          </Select>
        </div>

        <div>
          <label className="font-medium mb-3 block">Meal Type</label>
          <Select value={mealType} onValueChange={setMealType}>
            <SelectTrigger>
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Types</SelectItem>
              <SelectItem value="main">Main Course</SelectItem>
              <SelectItem value="appetizer">Appetizer</SelectItem>
              <SelectItem value="dessert">Dessert</SelectItem>
            </SelectContent>
          </Select>
        </div>

        <div>
          <label className="font-medium mb-3 block">Budget</label>
          <Select value={budget} onValueChange={setBudget}>
            <SelectTrigger>
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Budgets</SelectItem>
              <SelectItem value="low">Low</SelectItem>
              <SelectItem value="medium">Medium</SelectItem>
              <SelectItem value="high">High</SelectItem>
            </SelectContent>
          </Select>
        </div>
      </div>
    );
  };

  const renderBoardGameFilters = () => {
    const types = ['Strategy', 'Party', 'Cooperative', 'Card Game', 'Family'];
    
    return (
      <div className="space-y-6">
        <div>
          <label className="font-medium mb-3 block">Game Types</label>
          <div className="flex flex-wrap gap-2">
            {types.map((type) => (
              <Badge
                key={type}
                variant={gameTypes.includes(type) ? 'default' : 'outline'}
                className="cursor-pointer"
                onClick={() => toggleGenre(type, gameTypes, setGameTypes)}
              >
                {type}
              </Badge>
            ))}
          </div>
        </div>

        <div>
          <label className="font-medium mb-3 block">
            Number of Players: {players[0]} - {players[1]}
          </label>
          <Slider
            value={players}
            onValueChange={(val) => setPlayers(val as [number, number])}
            min={1}
            max={10}
            step={1}
          />
        </div>

        <div>
          <label className="font-medium mb-3 block">
            Minimum Rating: {minRating.toFixed(1)}+
          </label>
          <Slider
            value={[minRating]}
            onValueChange={(val) => setMinRating(val[0])}
            min={0}
            max={10}
            step={0.5}
          />
        </div>
      </div>
    );
  };

  const renderFilters = () => {
    switch (category) {
      case 'movies':
        return renderMovieFilters();
      case 'restaurants':
        return renderRestaurantFilters();
      case 'recipes':
        return renderRecipeFilters();
      case 'boardgames':
        return renderBoardGameFilters();
      default:
        return null;
    }
  };

  const getCategoryName = () => {
    const names = {
      movies: 'Movies & TV',
      restaurants: 'Restaurants',
      recipes: 'Recipes',
      boardgames: 'Board Games',
    };
    return names[category];
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
            <h1 className="text-2xl font-bold">Set Filters</h1>
            <p className="text-sm text-gray-600">{getCategoryName()}</p>
          </div>
        </div>
      </div>

      <div className="max-w-4xl mx-auto px-4 py-8 space-y-6">
        <Card className="p-6">
          {renderFilters()}
        </Card>

        <div className="flex space-x-4">
          <Button variant="outline" onClick={onBack} className="flex-1">
            Back
          </Button>
          <Button
            onClick={onStartSwipe}
            className="flex-1 bg-gradient-to-r from-pink-500 to-purple-500"
          >
            <Play className="mr-2 h-5 w-5" />
            Start Swiping
          </Button>
        </div>
      </div>
    </div>
  );
};
