import { useState, useEffect } from 'react';
import { wardrobeService } from '../services/api';

export const useWardrobe = () => {
  const [wardrobes, setWardrobes] = useState([]);
  const [topClothes, setTopClothes] = useState([]);
  const [bottomClothes, setBottomClothes] = useState([]);
  const [fullClothes, setFullClothes] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  // Fetch all wardrobes
  const fetchWardrobes = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await wardrobeService.getWardrobes();
      if (response.success) {
        setWardrobes(response.data);
      } else {
        setError(response.message || 'Failed to fetch wardrobes');
      }
    } catch (err) {
      setError(err.message || 'An error occurred while fetching wardrobes');
    } finally {
      setLoading(false);
    }
  };

  // Fetch filtered clothes by type
  const fetchFilteredClothes = async (type) => {
    try {
      setLoading(true);
      setError(null);
      const response = await wardrobeService.getFilteredClothes(type);
      if (response.success) {
        switch (type) {
          case 'top':
            setTopClothes(response.data);
            break;
          case 'bottom':
            setBottomClothes(response.data);
            break;
          case 'full':
            setFullClothes(response.data);
            break;
        }
      } else {
        setError(response.message || `Failed to fetch ${type} clothes`);
      }
    } catch (err) {
      setError(err.message || `An error occurred while fetching ${type} clothes`);
    } finally {
      setLoading(false);
    }
  };

  // Save new wardrobe
  const saveWardrobe = async (wardrobeData) => {
    try {
      setLoading(true);
      setError(null);

      // Validate that at least one clothing item is selected
      if (!wardrobeData.topClothesId && !wardrobeData.bottomClothesId && !wardrobeData.fullOutfitClothesId) {
        throw new Error('Please select at least one clothing item');
      }

      const response = await wardrobeService.saveWardrobe(wardrobeData);

      if (response.success) {
        setWardrobes(prev => [response.data, ...prev]);
        return { success: true, data: response.data };
      } else {
        throw new Error(response.message || 'Failed to save wardrobe');
      }
    } catch (err) {
      const errorMessage = err.response?.data?.message || err.message || 'An error occurred while saving wardrobe';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    } finally {
      setLoading(false);
    }
  };

  // Update existing wardrobe
  const updateWardrobe = async (wardrobeId, wardrobeData) => {
    try {
      setLoading(true);
      setError(null);

      // Validate that at least one clothing item is selected
      if (!wardrobeData.topClothesId && !wardrobeData.bottomClothesId && !wardrobeData.fullOutfitClothesId) {
        throw new Error('Please select at least one clothing item');
      }

      const response = await wardrobeService.updateWardrobe(wardrobeId, wardrobeData);

      if (response.success) {
        setWardrobes(prev => 
          prev.map(wardrobe => 
            wardrobe.id === wardrobeId ? response.data : wardrobe
          )
        );
        return { success: true, data: response.data };
      } else {
        throw new Error(response.message || 'Failed to update wardrobe');
      }
    } catch (err) {
      const errorMessage = err.response?.data?.message || err.message || 'An error occurred while updating wardrobe';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    } finally {
      setLoading(false);
    }
  };

  // Delete wardrobe
  const deleteWardrobe = async (wardrobeId) => {
    try {
      setLoading(true);
      setError(null);

      const response = await wardrobeService.deleteWardrobe(wardrobeId);

      if (response.success) {
        setWardrobes(prev => prev.filter(wardrobe => wardrobe.id !== wardrobeId));
        return { success: true };
      } else {
        throw new Error(response.message || 'Failed to delete wardrobe');
      }
    } catch (err) {
      const errorMessage = err.response?.data?.message || err.message || 'An error occurred while deleting wardrobe';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    } finally {
      setLoading(false);
    }
  };

  // Get single wardrobe
  const getWardrobe = async (wardrobeId) => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await wardrobeService.getWardrobe(wardrobeId);
      
      if (response.success) {
        return { success: true, data: response.data };
      } else {
        throw new Error(response.message || 'Failed to get wardrobe');
      }
    } catch (err) {
      const errorMessage = err.response?.data?.message || err.message || 'An error occurred while getting wardrobe';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    } finally {
      setLoading(false);
    }
  };

  // Clear error
  const clearError = () => {
    setError(null);
  };

  // Initialize data on mount
  useEffect(() => {
    fetchWardrobes();
  }, []);

  return {
    wardrobes,
    topClothes,
    bottomClothes,
    fullClothes,
    loading,
    error,
    fetchWardrobes,
    fetchFilteredClothes,
    saveWardrobe,
    updateWardrobe,
    deleteWardrobe,
    getWardrobe,
    clearError,
  };
};