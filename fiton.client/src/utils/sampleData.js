import api from '../utils/api';

// Utility function to add sample clothes data
export const addSampleClothesData = async () => {
  try {
    const response = await api.post('/clothes/seed-sample-data');
    return response.data;
  } catch (error) {
    throw new Error(error.response?.data?.error || 'Failed to add sample data');
  }
};