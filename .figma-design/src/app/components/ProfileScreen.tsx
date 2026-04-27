import React from 'react';
import { Card } from './ui/card';
import { Button } from './ui/button';
import { Avatar, AvatarFallback, AvatarImage } from './ui/avatar';
import { Badge } from './ui/badge';
import { ArrowLeft, Award, Star, MessageSquare, Trophy } from 'lucide-react';
import { useApp } from '../context/AppContext';

interface ProfileScreenProps {
  onBack: () => void;
}

export const ProfileScreen: React.FC<ProfileScreenProps> = ({ onBack }) => {
  const { currentUser, matchHistory } = useApp();

  if (!currentUser) return null;

  const stats = [
    {
      label: 'Matches',
      value: matchHistory.length,
      icon: Trophy,
      color: 'text-yellow-500',
    },
    {
      label: 'Reviews',
      value: currentUser.reviewCount,
      icon: MessageSquare,
      color: 'text-blue-500',
    },
    {
      label: 'Ratings',
      value: currentUser.ratingCount,
      icon: Star,
      color: 'text-purple-500',
    },
  ];

  return (
    <div className="min-h-screen bg-gradient-to-br from-pink-50 to-purple-50">
      {/* Header */}
      <div className="bg-white border-b">
        <div className="max-w-4xl mx-auto px-4 py-4 flex items-center space-x-4">
          <Button variant="ghost" size="icon" onClick={onBack}>
            <ArrowLeft className="h-5 w-5" />
          </Button>
          <h1 className="text-2xl font-bold">My Profile</h1>
        </div>
      </div>

      <div className="max-w-4xl mx-auto px-4 py-8 space-y-6">
        {/* Profile Card */}
        <Card className="p-8">
          <div className="flex flex-col items-center text-center space-y-4">
            <Avatar className="h-24 w-24">
              <AvatarImage src={currentUser.profilePicture} />
              <AvatarFallback className="text-2xl">
                {currentUser.username.substring(0, 2).toUpperCase()}
              </AvatarFallback>
            </Avatar>
            
            <div>
              <h2 className="text-2xl font-bold">{currentUser.username}</h2>
              <p className="text-gray-600">{currentUser.email}</p>
            </div>

            {currentUser.bio && (
              <p className="text-gray-700 max-w-md">{currentUser.bio}</p>
            )}
          </div>
        </Card>

        {/* Stats Grid */}
        <div className="grid grid-cols-3 gap-4">
          {stats.map((stat) => {
            const Icon = stat.icon;
            return (
              <Card key={stat.label} className="p-6 text-center">
                <Icon className={`h-8 w-8 mx-auto mb-2 ${stat.color}`} />
                <p className="text-3xl font-bold mb-1">{stat.value}</p>
                <p className="text-sm text-gray-600">{stat.label}</p>
              </Card>
            );
          })}
        </div>

        {/* Badges */}
        <Card className="p-6">
          <div className="flex items-center space-x-2 mb-4">
            <Award className="h-5 w-5 text-yellow-500" />
            <h3 className="font-semibold">Badges & Achievements</h3>
          </div>

          {currentUser.badges.length === 0 ? (
            <p className="text-gray-500 text-sm text-center py-8">
              No badges yet. Keep using the app to earn achievements!
            </p>
          ) : (
            <div className="space-y-3">
              {currentUser.badges.map((badge) => (
                <div
                  key={badge.type}
                  className="flex items-center space-x-4 p-4 bg-gradient-to-r from-yellow-50 to-orange-50 rounded-lg border border-yellow-200"
                >
                  <div className="bg-yellow-400 p-3 rounded-full">
                    <Award className="h-6 w-6 text-white" />
                  </div>
                  <div className="flex-1">
                    <p className="font-semibold">{badge.name}</p>
                    <p className="text-sm text-gray-600">{badge.description}</p>
                  </div>
                </div>
              ))}
            </div>
          )}
        </Card>

        {/* Progress to Next Badge */}
        <Card className="p-6 bg-gradient-to-r from-purple-50 to-pink-50">
          <h3 className="font-semibold mb-4">Progress to Next Achievement</h3>
          
          <div className="space-y-4">
            {currentUser.reviewCount < 50 && (
              <div>
                <div className="flex justify-between text-sm mb-2">
                  <span>Reviewer Level 2</span>
                  <span className="text-gray-600">{currentUser.reviewCount}/50 reviews</span>
                </div>
                <div className="h-2 bg-gray-200 rounded-full overflow-hidden">
                  <div
                    className="h-full bg-gradient-to-r from-purple-500 to-pink-500"
                    style={{ width: `${(currentUser.reviewCount / 50) * 100}%` }}
                  />
                </div>
              </div>
            )}

            {currentUser.ratingCount < 100 && (
              <div>
                <div className="flex justify-between text-sm mb-2">
                  <span>Super User</span>
                  <span className="text-gray-600">{currentUser.ratingCount}/100 ratings</span>
                </div>
                <div className="h-2 bg-gray-200 rounded-full overflow-hidden">
                  <div
                    className="h-full bg-gradient-to-r from-purple-500 to-pink-500"
                    style={{ width: `${(currentUser.ratingCount / 100) * 100}%` }}
                  />
                </div>
              </div>
            )}

            {matchHistory.length < 25 && (
              <div>
                <div className="flex justify-between text-sm mb-2">
                  <span>Match Master</span>
                  <span className="text-gray-600">{matchHistory.length}/25 matches</span>
                </div>
                <div className="h-2 bg-gray-200 rounded-full overflow-hidden">
                  <div
                    className="h-full bg-gradient-to-r from-purple-500 to-pink-500"
                    style={{ width: `${(matchHistory.length / 25) * 100}%` }}
                  />
                </div>
              </div>
            )}
          </div>
        </Card>
      </div>
    </div>
  );
};
