import React from 'react';
import { Card } from './ui/card';
import { Button } from './ui/button';
import { X, Heart, Users, Film, Trophy } from 'lucide-react';

interface WelcomeTourProps {
  onClose: () => void;
}

export const WelcomeTour: React.FC<WelcomeTourProps> = ({ onClose }) => {
  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center p-4 z-50">
      <Card className="max-w-2xl w-full max-h-[90vh] overflow-y-auto p-8">
        <div className="flex justify-between items-start mb-6">
          <div>
            <h2 className="text-2xl font-bold mb-2">Welcome to SwipeMate! 🎉</h2>
            <p className="text-gray-600">Make group decisions together</p>
          </div>
          <Button variant="ghost" size="icon" onClick={onClose}>
            <X className="h-5 w-5" />
          </Button>
        </div>

        <div className="space-y-6">
          <div className="flex items-start space-x-4">
            <div className="bg-gradient-to-br from-pink-500 to-purple-500 p-3 rounded-full flex-shrink-0">
              <Film className="h-6 w-6 text-white" />
            </div>
            <div>
              <h3 className="font-semibold mb-1">Choose a Category</h3>
              <p className="text-sm text-gray-600">
                Pick from Movies, Restaurants, Recipes, or Board Games to start matching.
              </p>
            </div>
          </div>

          <div className="flex items-start space-x-4">
            <div className="bg-gradient-to-br from-blue-500 to-cyan-500 p-3 rounded-full flex-shrink-0">
              <Users className="h-6 w-6 text-white" />
            </div>
            <div>
              <h3 className="font-semibold mb-1">Select Friends</h3>
              <p className="text-sm text-gray-600">
                Choose which friends to match with, or browse solo. You can add friends from the Friends screen.
              </p>
            </div>
          </div>

          <div className="flex items-start space-x-4">
            <div className="bg-gradient-to-br from-green-500 to-emerald-500 p-3 rounded-full flex-shrink-0">
              <Heart className="h-6 w-6 text-white" fill="white" />
            </div>
            <div>
              <h3 className="font-semibold mb-1">Swipe to Match</h3>
              <p className="text-sm text-gray-600">
                Swipe right or tap ❤️ to like, swipe left or tap ✕ to pass. When you like something, it's added to your matches!
              </p>
            </div>
          </div>

          <div className="flex items-start space-x-4">
            <div className="bg-gradient-to-br from-orange-500 to-red-500 p-3 rounded-full flex-shrink-0">
              <Trophy className="h-6 w-6 text-white" />
            </div>
            <div>
              <h3 className="font-semibold mb-1">Track Your Progress</h3>
              <p className="text-sm text-gray-600">
                View your match history, check your profile stats, and earn badges by being active.
              </p>
            </div>
          </div>

          <div className="bg-gradient-to-r from-purple-50 to-pink-50 p-4 rounded-lg border border-purple-200">
            <p className="text-sm text-gray-700">
              <strong>Demo Mode:</strong> This is a frontend prototype. In a production version with backend,
              all friends would swipe in real-time and you'd get notified when everyone matches!
            </p>
          </div>

          <Button
            onClick={onClose}
            className="w-full bg-gradient-to-r from-pink-500 to-purple-500"
          >
            Get Started
          </Button>
        </div>
      </Card>
    </div>
  );
};
